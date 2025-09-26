// src/features/admin/AdminDashboard.tsx
import React, { useEffect, useState } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { fetchVets, type VetItem } from "./vetsApi";
import { fetchAdminUsers, type UserListItem } from "./usersApi";
import { FaArrowRight } from "react-icons/fa";

const AdminDashboard: React.FC = () => {
  const navigate = useNavigate();
  const [searchTerm] = useState("");
  const [vets, setVets] = useState<VetItem[]>([]);
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const stats = {
    totalVets: vets.length,
    totalUsers: users.length,
    totalPets: 0,
    pendingAppointments: 0,
    lowStockItems: 0,
    monthlyRevenue: 0,
    todayAppointments: 0,
  };

  useEffect(() => {
    let alive = true;
    (async () => {
      setLoading(true);
      setError(null);
      const [vetRes, userRes] = await Promise.all([
        fetchVets({ page: 1, pageSize: 3, search: searchTerm }),
        fetchAdminUsers({ page: 1, pageSize: 3 }),
      ]);
      if (!alive) return;

      if (vetRes.ok) setVets(vetRes.data.items);
      else setError(vetRes.detail || "Failed to load vets.");

      if (userRes.ok) setUsers(userRes.items);
      else setError(userRes.detail || "Failed to load users.");

      setLoading(false);
    })();
    return () => {
      alive = false;
    };
  }, [searchTerm]);

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case "active":
        return "bg-green-100 text-green-700";
      case "inactive":
        return "bg-red-100 text-red-700";
      default:
        return "bg-gray-100 text-gray-700";
    }
  };

  return (
    <>
      <h2 className="text-3xl font-bold text-blue-800 mb-2">Admin Dashboard</h2>
      <p className="text-gray-600 mb-6">
        Manage your veterinary hospital operations 🐶🐱
      </p>

      {/* Stats */}
      <div className="grid gap-6 md:grid-cols-3 lg:grid-cols-6 mb-8">
        {[
          {
            label: "Total Vets",
            value: stats.totalVets,
            border: "border-blue-500",
          },
          {
            label: "Total Users",
            value: stats.totalUsers,
            border: "border-teal-500",
          },
          {
            label: "Today's Appointments",
            value: stats.todayAppointments,
            border: "border-orange-500",
          },
          {
            label: "Pending Requests",
            value: stats.pendingAppointments,
            border: "border-yellow-500",
          },
          {
            label: "Low Stock Items",
            value: stats.lowStockItems,
            border: "border-purple-500",
          },
          {
            label: "Monthly Revenue",
            value: `$${stats.monthlyRevenue}`,
            border: "border-pink-500",
          },
        ].map((item, i) => (
          <div
            key={i}
            className={`bg-white rounded-xl shadow-md p-4 border-t-4 ${item.border}`}
          >
            <p className="text-sm text-gray-500">{item.label}</p>
            <p className="text-2xl font-bold text-blue-900">{item.value}</p>
          </div>
        ))}
      </div>

      {/* Vet Preview */}
      <div className="bg-white rounded-xl shadow-md p-6 mb-8 border-l-4 border-blue-500">
        <div className="flex justify-between items-center mb-4">
          <h3 className="font-semibold text-lg text-blue-700">
            Vet Management
          </h3>
          <Link
            to="/admin/vets"
            className="px-4 py-2 bg-green-500 hover:opacity-90 text-white rounded-md text-sm"
          >
            View All
          </Link>
        </div>
        {loading && <p className="text-gray-500">Loading vets…</p>}
        {error && <p className="text-red-600">{error}</p>}
        <div className="space-y-3">
          {vets.map((vet) => (
            <div
              key={vet.id}
              className="flex justify-between items-center border rounded-lg p-4 bg-gradient-to-r from-blue-50 to-teal-50"
            >
              <div>
                <h4 className="font-semibold">{vet.fullName}</h4>
                <p className="text-sm text-gray-500">{vet.email}</p>
                <span
                  className={`inline-block text-xs px-2 py-1 rounded ${getStatusColor(
                    "active"
                  )}`}
                >
                  Vet • Active
                </span>
              </div>
              <button
                onClick={() => navigate(`/admin/vets/${vet.id}`)}
                className="text-blue-600 text-xl"
              >
                <FaArrowRight />
              </button>
            </div>
          ))}
        </div>
      </div>

      {/* User Preview */}
      <div className="bg-white rounded-xl shadow-md p-6 border-l-4 border-teal-500">
        <div className="flex justify-between items-center mb-4">
          <h3 className="font-semibold text-lg text-teal-700">
            User Management
          </h3>
          <Link
            to="/admin/users"
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md text-sm"
          >
            View All
          </Link>
        </div>
        {loading && <p className="text-gray-500">Loading users…</p>}
        {error && <p className="text-red-600">{error}</p>}
        <div className="space-y-3">
          {users.map((u) => (
            <div
              key={u.id}
              className="flex justify-between items-center border rounded-lg p-4 bg-gradient-to-r from-teal-50 to-blue-50"
            >
              <div>
                <h4 className="font-semibold">{u.fullName || "—"}</h4>
                <p className="text-sm text-gray-500">{u.email}</p>
                <span
                  className={`inline-block text-xs px-2 py-1 rounded ${getStatusColor(
                    u.isActive ? "active" : "inactive"
                  )}`}
                >
                  {u.isActive ? "Active" : "Inactive"}
                </span>
              </div>
              <button
                onClick={() => navigate(`/admin/users/${u.id}`)}
                className="text-blue-600 text-xl"
              >
                <FaArrowRight />
              </button>
            </div>
          ))}
        </div>
      </div>
    </>
  );
};

export default AdminDashboard;
