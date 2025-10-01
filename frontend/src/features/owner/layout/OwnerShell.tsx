// src/features/owner/layout/OwnerShell.tsx
import { useEffect, useState } from "react";
import { Outlet, Link } from "react-router-dom";
import OwnerSidebar from "./OwnerSidebar";

export default function OwnerShell() {
  // persisted collapse state
  const [isCollapsed, setIsCollapsed] = useState<boolean>(() => {
    const saved = localStorage.getItem("owner_sidebar_collapsed");
    return saved === "true";
  });

  // mobile drawer state (independent from collapsed)
  const [drawerOpen, setDrawerOpen] = useState(false);

  useEffect(() => {
    localStorage.setItem("owner_sidebar_collapsed", String(isCollapsed));
  }, [isCollapsed]);

  return (
    <div className="min-h-screen flex bg-gradient-to-b from-white via-slate-50 to-white text-slate-800">
      {/* Sidebar (desktop + mobile) */}
      <OwnerSidebar
        isCollapsed={isCollapsed}
        onToggleCollapse={() => setIsCollapsed((v) => !v)}
        drawerOpen={drawerOpen}
        onCloseDrawer={() => setDrawerOpen(false)}
      />

      {/* Content */}
      <div className={`flex-1 ${isCollapsed ? "md:ml-20" : "md:ml-64"}`}>
        {/* Top bar */}
        <header className="sticky top-0 z-40 backdrop-blur supports-[backdrop-filter]:bg-white/70 border-b border-slate-200/60">
          <div className="h-16 flex items-center justify-between px-4 sm:px-6 lg:px-8">
            <button
              className="md:hidden inline-flex items-center justify-center rounded-lg p-2 hover:bg-slate-100 focus-visible:ring focus-visible:ring-green-500"
              onClick={() => setDrawerOpen(true)}
              aria-label="Open menu"
            >
              ☰
            </button>

            <Link
              to="/owner"
              className="font-semibold tracking-tight text-base sm:text-lg"
            >
              PetCare+ Owner Portal
            </Link>

            <button
              className="inline-flex items-center rounded-2xl bg-green-600 px-4 py-2 text-sm font-semibold text-white hover:bg-green-700 focus-visible:ring focus-visible:ring-green-500"
              onClick={() => {
                localStorage.clear();
                window.location.assign("/");
              }}
            >
              Logout
            </button>
          </div>
        </header>

        {/* Routed page content */}
        <main className="mx-auto w-full max-w-7xl py-8 px-4 sm:px-6 lg:px-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}