using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using Xunit;

namespace PetCare.Ui.Tests
{
    public class InventoryUiTests : IAsyncLifetime, IDisposable
    {
        private IWebDriver? _driver;
        private readonly string _baseUrl;
        private readonly TimeSpan _waitTimeout = TimeSpan.FromSeconds(20); // slightly larger base timeout

        // Admin credentials (env vars optional)
        private readonly string _adminEmail = Environment.GetEnvironmentVariable("TEST_ADMIN_EMAIL") ?? "admin@admin.com";
        private readonly string _adminPassword = Environment.GetEnvironmentVariable("TEST_ADMIN_PASSWORD") ?? "Admin1234";

        public InventoryUiTests()
        {
            _baseUrl = Environment.GetEnvironmentVariable("PETCARE_UI_URL") ?? "http://localhost:5173";
        }

        public Task InitializeAsync()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());

            var options = new ChromeOptions();
            var headless = (Environment.GetEnvironmentVariable("HEADLESS") ?? "false").ToLower() == "true";
            if (headless) options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--window-size=1600,1000");

            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _driver?.Quit();
            _driver?.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

        [Fact(DisplayName = "Inventory page loads and list is visible")]
        public void InventoryPage_Loads_ListVisible()
        {
            try
            {
                NavigateToInventoryPage();
            }
            catch (Exception)
            {
                SaveDebugArtifacts("inventory_load_failure");
                throw;
            }

            var wait = new WebDriverWait(_driver!, _waitTimeout);
            var header = wait.Until(driver =>
            {
                try
                {
                    var h = driver.FindElements(By.XPath("//h1[contains(normalize-space(.), 'Inventory Management')]"));
                    return h.Count > 0 ? h[0] : null;
                }
                catch { return null; }
            });
            Assert.NotNull(header);

            var card = wait.Until(driver =>
            {
                try
                {
                    var cards = driver.FindElements(By.CssSelector("div.ant-card, div.admin-inventory, #admin-inventory-list"));
                    return cards.Count > 0 ? cards[0] : null;
                }
                catch { return null; }
            });
            Assert.NotNull(card);
        }

        [Fact(DisplayName = "Create inventory item (happy path)")]
        public void Inventory_CreateItem_HappyPath()
        {
            try
            {
                NavigateToInventoryPage();
            }
            catch (Exception)
            {
                SaveDebugArtifacts("inventory_create_nav_failure");
                throw;
            }

            var wait = new WebDriverWait(_driver!, _waitTimeout);

            var addBtn = TryFindFirst(wait, new[]
            {
                By.XPath("//button[normalize-space(.)='Add Item']"),
                By.XPath("//button[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'), 'add item')]"),
                By.CssSelector("button[data-testid='new-item']"),
                By.CssSelector("button#add-inventory")
            });

            if (addBtn == null)
            {
                SaveDebugArtifacts("add_button_missing");
                throw new InvalidOperationException("Add Item button not found - update selector.");
            }

            addBtn.Click();

            // Wait for modal to appear (search for normal AntD modal or generic dialog)
            IWebElement? modal = new WebDriverWait(_driver!, _waitTimeout).Until(driver =>
            {
                try
                {
                    var m = driver.FindElements(By.CssSelector(".ant-modal, .modal, [role='dialog']"));
                    return m.Count > 0 ? m[0] : null;
                }
                catch { return null; }
            });

            if (modal == null)
            {
                SaveDebugArtifacts("modal_missing_after_add");
                throw new InvalidOperationException("Modal did not open after clicking Add Item.");
            }

            // --- SAVE modal DOM immediately for debugging (guarantees artifact) ---
            try
            {
                var outDir = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "Screenshots");
                Directory.CreateDirectory(outDir);
                var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var modalHtml = modal.GetAttribute("outerHTML") ?? modal.GetAttribute("innerHTML") ?? _driver!.PageSource ?? "";
                var modalPath = Path.Combine(outDir, $"modal_dom_before_confirm_{stamp}.html");
                File.WriteAllText(modalPath, modalHtml);
                var pagePath = Path.Combine(outDir, $"page_after_modal_{stamp}.html");
                File.WriteAllText(pagePath, _driver!.PageSource ?? "");
            }
            catch
            {
                // best-effort only
            }

            var uniqueName = "E2E Item " + Guid.NewGuid().ToString("N").Substring(0, 6);

            try
            {
                IWebElement? nameEl = null;
                var nameCandidates = modal.FindElements(By.CssSelector("input[placeholder='Name'], input[aria-label='Name'], input[name='name']"));
                if (nameCandidates.Count > 0) nameEl = nameCandidates[0];

                IWebElement? qtyEl = null;
                var qtyCandidates = modal.FindElements(By.CssSelector("input[placeholder='Quantity'], input[type='number'], input[name='quantity']"));
                if (qtyCandidates.Count > 0) qtyEl = qtyCandidates[0];

                IWebElement? supplierEl = null;
                var supplierCandidates = modal.FindElements(By.CssSelector("input[placeholder='Supplier'], input[name='supplier']"));
                if (supplierCandidates.Count > 0) supplierEl = supplierCandidates[0];

                IWebElement? categorySelect = null;
                var categoryCandidates = modal.FindElements(By.CssSelector(".ant-select, select[data-testid='category']"));
                if (categoryCandidates.Count > 0) categorySelect = categoryCandidates[0];

                if (nameEl != null)
                {
                    nameEl.Clear();
                    nameEl.SendKeys(uniqueName);
                }

                if (qtyEl != null)
                {
                    qtyEl.Clear();
                    qtyEl.SendKeys("3");
                }

                if (supplierEl != null)
                {
                    supplierEl.Clear();
                    supplierEl.SendKeys("E2E Supplier");
                }

                if (categorySelect != null && categorySelect.TagName.ToLower() != "select")
                {
                    categorySelect.Click();
                    Thread.Sleep(400);
                    var opt = _driver!.FindElements(By.CssSelector(".ant-select-item-option-content"));
                    if (opt.Count > 0) opt[0].Click();
                }
            }
            catch
            {
                // best-effort
            }

            // save modal HTML again (optional)
            try
            {
                var outDir = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "Screenshots");
                var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var modalHtml2 = modal.GetAttribute("outerHTML") ?? "";
                File.WriteAllText(Path.Combine(outDir, $"modal_dom_before_click_{stamp}.html"), modalHtml2);
            }
            catch { }

            // robust confirm polling + click
            IWebElement? confirmBtn = null;
            var confirmTimeout = TimeSpan.FromSeconds(30); // increased
            var confirmDeadline = DateTime.UtcNow + confirmTimeout;

            while (DateTime.UtcNow < confirmDeadline && confirmBtn == null)
            {
                try
                {
                    // 1) try modal footer buttons
                    var footBtns = modal.FindElements(By.CssSelector(".ant-modal .ant-modal-footer button, .modal-footer button, .ant-modal-footer button"));
                    foreach (var b in footBtns)
                    {
                        if (b.Displayed && b.Enabled) { confirmBtn = b; break; }
                    }

                    // 2) try common text buttons inside modal
                    if (confirmBtn == null)
                    {
                        var textBtns = modal.FindElements(By.XPath(".//button[normalize-space(.)='Create' or normalize-space(.)='Add' or normalize-space(.)='Save' or normalize-space(.)='OK' or normalize-space(.)='Confirm' or normalize-space(.)='Add Item']"));
                        if (textBtns.Count > 0) { confirmBtn = textBtns[0]; if (!confirmBtn.Displayed || !confirmBtn.Enabled) confirmBtn = null; }
                    }

                    // 3) primary-styled button
                    if (confirmBtn == null)
                    {
                        var primary = modal.FindElements(By.CssSelector(".ant-modal .ant-btn.ant-btn-primary, button.ant-btn-primary"));
                        if (primary.Count > 0 && primary[0].Displayed && primary[0].Enabled) confirmBtn = primary[0];
                    }

                    // 4) fallback: any visible button inside modal
                    if (confirmBtn == null)
                    {
                        var any = modal.FindElements(By.CssSelector("button"));
                        foreach (var b in any) { if (b.Displayed && b.Enabled) { confirmBtn = b; break; } }
                    }
                }
                catch { }

                if (confirmBtn == null) Thread.Sleep(300);
            }

            if (confirmBtn == null)
            {
                SaveDebugArtifacts("confirm_button_missing_after_poll");
                throw new InvalidOperationException("Confirm button not found inside modal after polling. Inspect the saved modal DOM file in TestResults/Screenshots.");
            }

            // try click with JS fallback
            try
            {
                ((IJavaScriptExecutor)_driver!).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", confirmBtn);
                var waitClickable = new WebDriverWait(_driver!, TimeSpan.FromSeconds(12));
                waitClickable.Until(d => confirmBtn.Displayed && confirmBtn.Enabled);
                confirmBtn.Click();
            }
            catch (ElementClickInterceptedException)
            {
                try { ((IJavaScriptExecutor)_driver!).ExecuteScript("arguments[0].click();", confirmBtn); }
                catch { SaveDebugArtifacts("confirm_click_failed_js"); throw new InvalidOperationException("Failed to click confirm button."); }
            }
            catch (WebDriverException)
            {
                try { ((IJavaScriptExecutor)_driver!).ExecuteScript("arguments[0].click();", confirmBtn); }
                catch { SaveDebugArtifacts("confirm_click_failed_unknown"); throw new InvalidOperationException("Failed to click confirm button."); }
            }

            // wait for modal closure (best-effort)
            try
            {
                var closedOk = new WebDriverWait(_driver!, TimeSpan.FromSeconds(18)).Until(d =>
                {
                    try { return d.FindElements(By.CssSelector(".ant-modal, .modal, [role='dialog']")).Count == 0; } catch { return false; }
                });
            }
            catch
            {
                SaveDebugArtifacts("modal_not_closed_after_confirm");
            }

            // longer polling for created item
            var createdDeadline = DateTime.UtcNow + TimeSpan.FromSeconds(45);
            IWebElement? found = null;
            while (DateTime.UtcNow < createdDeadline && found == null)
            {
                try
                {
                    var elements = _driver!.FindElements(By.XPath($"//div[contains(@class,'ant-card') or contains(@class,'admin-inventory')]//*[contains(text(), '{uniqueName}')]"));
                    if (elements.Count > 0) { found = elements[0]; break; }
                    var anywhere = _driver.FindElements(By.XPath($"//*[contains(text(), '{uniqueName}')]"));
                    if (anywhere.Count > 0) { found = anywhere[0]; break; }
                }
                catch { }
                Thread.Sleep(500);
            }

            if (found == null)
            {
                SaveDebugArtifacts("created_item_not_found_after_confirm_poll");
                throw new InvalidOperationException("Created item not found after confirming modal. Check backend/API and UI selectors. The modal DOM was saved to TestResults/Screenshots.");
            }

            Assert.NotNull(found);
        }

        // --- Navigation + helpers ---
        private void NavigateToInventoryPage()
        {
            var inventoryRoute = new Uri(new Uri(_baseUrl), "/admin/inventory").ToString();

            try { _driver!.Navigate().GoToUrl(inventoryRoute); }
            catch (WebDriverException ex) { SaveDebugArtifacts("navigate_error"); throw new InvalidOperationException($"Failed to navigate to {inventoryRoute}: {ex.Message}", ex); }

            if (IsLoginPageShown())
            {
                PerformLogin();
                _driver!.Navigate().GoToUrl(inventoryRoute);
            }

            var pageSource = _driver!.PageSource ?? string.Empty;
            if (pageSource.Contains("Not Found", StringComparison.OrdinalIgnoreCase) ||
                pageSource.Contains("Cannot GET", StringComparison.OrdinalIgnoreCase) ||
                pageSource.Contains("404"))
            {
                _driver.Navigate().GoToUrl(_baseUrl);
                var wait = new WebDriverWait(_driver!, _waitTimeout);
                var nav = TryFindFirst(wait, new[]
                {
                    By.CssSelector("a[href='/admin/inventory']"),
                    By.XPath("//a[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'), 'admin') and contains(., 'inventory')]"),
                    By.CssSelector("a[data-testid='nav-admin-inventory']"),
                    By.CssSelector("button[data-testid='nav-admin-inventory']")
                });

                if (nav == null) { SaveDebugArtifacts("nav_link_missing"); throw new InvalidOperationException("Could not navigate to /admin/inventory: fallback nav link not found."); }

                nav.Click();

                var waitFinal = new WebDriverWait(_driver!, _waitTimeout);
                var present = waitFinal.Until(driver =>
                {
                    try
                    {
                        var candidates = new[]
                        {
                            By.XPath("//h1[contains(normalize-space(.), 'Inventory Management')]"),
                            By.CssSelector("div.ant-card"),
                            By.CssSelector("div.admin-inventory")
                        };
                        foreach (var c in candidates)
                        {
                            var elems = driver.FindElements(c);
                            if (elems.Count > 0) return elems[0];
                        }
                    }
                    catch { }
                    return null;
                });

                if (present == null) { SaveDebugArtifacts("inventory_not_present_after_nav"); throw new InvalidOperationException("Inventory page did not load after clicking nav link."); }
            }
        }

        private bool IsLoginPageShown()
        {
            try
            {
                var page = _driver!.PageSource ?? string.Empty;
                if (page.IndexOf("login", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (page.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0 || page.IndexOf("email", StringComparison.OrdinalIgnoreCase) >= 0))
                    return true;

                var emailSelectors = new[]
                {
                    By.CssSelector("input[name='email']"),
                    By.CssSelector("input#email"),
                    By.CssSelector("input[name='username']"),
                    By.CssSelector("input[type='email']")
                };
                var pwdSelectors = new[]
                {
                    By.CssSelector("input[name='password']"),
                    By.CssSelector("input#password"),
                    By.CssSelector("input[type='password']")
                };

                foreach (var s in emailSelectors) if (_driver!.FindElements(s).Count > 0) return true;
                foreach (var s in pwdSelectors) if (_driver!.FindElements(s).Count > 0) return true;

                return false;
            }
            catch { return false; }
        }

        private void PerformLogin()
        {
            var wait = new WebDriverWait(_driver!, _waitTimeout);

            var emailEl = TryFindFirst(wait, new[]
            {
                By.CssSelector("input[name='email']"),
                By.CssSelector("input#email"),
                By.CssSelector("input[name='username']"),
                By.CssSelector("input[type='email']")
            });

            var pwdEl = TryFindFirst(wait, new[]
            {
                By.CssSelector("input[name='password']"),
                By.CssSelector("input#password"),
                By.CssSelector("input[type='password']")
            });

            if (emailEl == null || pwdEl == null) { SaveDebugArtifacts("login_fields_missing"); throw new InvalidOperationException("Login fields not found on page."); }

            emailEl.Clear(); emailEl.SendKeys(_adminEmail);
            pwdEl.Clear(); pwdEl.SendKeys(_adminPassword);

            var submitBtn = TryFindFirst(wait, new[]
            {
                By.CssSelector("button[type='submit']"),
                By.CssSelector("button[data-testid='login']"),
                By.XPath("//button[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'), 'login') or contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'), 'sign in')]")
            });

            if (submitBtn != null) submitBtn.Click(); else pwdEl.SendKeys(Keys.Enter);

            var loggedIn = new WebDriverWait(_driver!, TimeSpan.FromSeconds(18)).Until(dr => { try { return !IsLoginPageShown(); } catch { return false; } });
            if (!loggedIn) { SaveDebugArtifacts("login_failed"); throw new InvalidOperationException("Login did not complete successfully within timeout."); }
        }

        private IWebElement? TryFindFirst(WebDriverWait wait, By[] selectors)
        {
            try
            {
                return wait.Until(driver =>
                {
                    foreach (var s in selectors)
                    {
                        var els = driver.FindElements(s);
                        if (els != null && els.Count > 0) return els[0];
                    }
                    return null;
                });
            }
            catch { return null; }
        }

        private void SaveDebugArtifacts(string tag)
        {
            try
            {
                var outDir = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "Screenshots");
                Directory.CreateDirectory(outDir);
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                if (_driver != null)
                {
                    try { var screenshot = ((ITakesScreenshot)_driver).GetScreenshot(); File.WriteAllBytes(Path.Combine(outDir, $"{tag}_{timestamp}.png"), screenshot.AsByteArray); } catch {}
                    try { var html = _driver.PageSource; File.WriteAllText(Path.Combine(outDir, $"{tag}_{timestamp}.html"), html); } catch {}
                }
            }
            catch { }
        }
    }
}
