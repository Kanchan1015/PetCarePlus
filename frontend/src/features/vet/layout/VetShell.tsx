// src/features/vet/layout/VetShell.tsx
import { useEffect, useState } from "react";
import { Outlet, Link } from "react-router-dom";
import VetSidebar from "./VetSidebar";

export default function VetShell() {
  // persisted collapse state
  const [isCollapsed, setIsCollapsed] = useState<boolean>(() => {
    const saved = localStorage.getItem("vet_sidebar_collapsed");
    return saved === "true";
  });

  // mobile drawer state (independent from collapsed)
  const [drawerOpen, setDrawerOpen] = useState(false);

  useEffect(() => {
    localStorage.setItem("vet_sidebar_collapsed", String(isCollapsed));
  }, [isCollapsed]);

  return (
    <div className="min-h-screen flex bg-gradient-to-b from-white via-slate-50 to-white text-slate-800">
      {/* Sidebar (desktop + mobile) */}
      <VetSidebar
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
              className="md:hidden inline-flex items-center justify-center rounded-lg p-2 hover:bg-slate-100 focus-visible:ring focus-visible:ring-blue-500"
              onClick={() => setDrawerOpen(true)}
              aria-label="Open menu"
            >
              ☰
            </button>

            <Link
              to="/vet"
              className="font-semibold tracking-tight text-base sm:text-lg"
            >
              PetCare+ Veterinarian
            </Link>

            <button
              className="inline-flex items-center rounded-2xl bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 focus-visible:ring focus-visible:ring-blue-500"
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