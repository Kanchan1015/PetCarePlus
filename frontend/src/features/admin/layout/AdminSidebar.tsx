// src/features/admin/layout/AdminSidebar.tsx
import { NavLink } from "react-router-dom";
import {
  FaChevronLeft,
  FaChevronRight,
  FaUserMd,
  FaUsers,
  FaHome,
  FaPaw,
  FaCalendarAlt,
  FaBoxes,
} from "react-icons/fa";

type Props = {
  isCollapsed: boolean;
  onToggleCollapse: () => void;
  drawerOpen: boolean;
  onCloseDrawer: () => void;
};

const menu = [
  { label: "Dashboard", to: "/admin", icon: <FaHome /> },
  { label: "Vet Management", to: "/admin/vets", icon: <FaUserMd /> },
  { label: "User Management", to: "/admin/users", icon: <FaUsers /> },
  { label: "Pets", to: "/admin/pets", icon: <FaPaw /> },
  { label: "Appointments", to: "/admin/appointments", icon: <FaCalendarAlt /> },
  { label: "Inventory", to: "/admin/inventory", icon: <FaBoxes /> },
];

export default function AdminSidebar({
  isCollapsed,
  onToggleCollapse,
  drawerOpen,
  onCloseDrawer,
}: Props) {
  // landing-page aligned surface
  const base =
    "text-slate-800 bg-white/80 backdrop-blur supports-[backdrop-filter]:bg-white/80 border-r border-slate-200/60 shadow-sm z-40 p-3 md:p-4 h-full";
  const width = isCollapsed ? "w-20" : "w-64";

  const SidebarBody = (
    <aside className={`${base} ${width} flex flex-col`}>
      {/* Header + collapse toggle */}
      <div className="flex items-center justify-between mb-4">
        <div
          className={`font-semibold tracking-tight ${
            isCollapsed ? "text-base" : "text-xl"
          } truncate`}
        >
          {isCollapsed ? "PC+" : "Dashboard"}
        </div>
        <button
          className="hidden md:inline-flex items-center justify-center rounded-lg px-2 py-1 hover:bg-slate-100 focus-visible:ring focus-visible:ring-indigo-500"
          onClick={onToggleCollapse}
          aria-label={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
          aria-expanded={!isCollapsed}
          title={isCollapsed ? "Expand" : "Collapse"}
        >
          {isCollapsed ? <FaChevronRight /> : <FaChevronLeft />}
        </button>
      </div>

      {/* Nav */}
      <nav className="space-y-2 flex-1">
        {menu.map((m) => (
          <NavLink
            key={m.to}
            to={m.to}
            className={({ isActive }) =>
              [
                // base item style
                "group flex items-center gap-3 px-3 py-2 rounded-xl transition focus-visible:ring focus-visible:ring-indigo-500",
                // default / hover
                "hover:bg-slate-100",
                // active state → primary pill
                isActive ? "bg-indigo-600 text-white shadow" : "text-slate-700",
                // collapsed centers icon
                isCollapsed ? "justify-center" : "",
              ].join(" ")
            }
            onClick={onCloseDrawer}
          >
            <span className="text-lg">{m.icon}</span>
            {!isCollapsed && <span className="font-medium">{m.label}</span>}
          </NavLink>
        ))}
      </nav>
    </aside>
  );

  return (
    <>
      {/* Desktop fixed sidebar */}
      <div className="hidden md:block fixed inset-y-0 left-0">
        {SidebarBody}
      </div>

      {/* Mobile drawer */}
      {drawerOpen && (
        <div className="md:hidden fixed inset-0 z-50">
          <div
            className="absolute inset-0 bg-black/40"
            onClick={onCloseDrawer}
            aria-hidden="true"
          />
          <div
            className="absolute inset-y-0 left-0"
            role="dialog"
            aria-modal="true"
          >
            {SidebarBody}
          </div>
        </div>
      )}
    </>
  );
}
