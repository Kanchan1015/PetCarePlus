// App.tsx (only the Routes section shown for brevity)
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import LoginPage from "./features/auth/LoginPage.tsx";
import RegistrationPage from "./features/auth/RegistrationPage.tsx";
import OwnerPage from "./features/owner/OwnerPage.tsx";
import VetPage from "./features/vet/VetPage.tsx";
import AdminDashboard from "./features/admin/AdminDashboard.tsx";
import Unauthorized from "./pages/Unauthorized.tsx";
import NotFound from "./pages/NotFound.tsx";
import RequireAdmin from "./features/auth/RequireAdmin";
import RequireAuth from "./features/auth/RequireAuth";
import RequireRole from "./features/auth/RequireRole";
import AdminAddVetPage from "./features/admin/AdminAddVetPage.tsx";
import AdminVetListPage from "./features/admin/AdminVetListPage.tsx";
import AdminVetDetailsPage from "./features/admin/AdminVetDetailsPage.tsx";
import AdminUsersPage from "./features/admin/AdminUsersPage";
import AdminUserEditPage from "./features/admin/AdminUserEditPage";
import OwnerProfilePage from "./features/owner/OwnerProfilePage";
import { AdminPetsPage, AdminPetAddPage, AdminPetEditPage } from "./features/admin/pages";
import { OwnerPetsPage } from "./features/owner/pages";
import LandingPage from "./pages/LandingPage.tsx";
import AdminShell from "./features/admin/layout/AdminShell"; //

const App = () => {
  return (
    <Router>
      <Routes>
        {/* Public */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegistrationPage />} />
        <Route path="/" element={<LandingPage />} />

        {/* Owner/Vet */}
        <Route
          path="/owner"
          element={
            <RequireAuth>
              <RequireRole roles={["OWNER", "ADMIN", "VET"]}>
                <OwnerPage />
              </RequireRole>
            </RequireAuth>
          }
        />
        <Route
          path="/vet"
          element={
            <RequireAuth>
              <RequireRole roles={["VET", "ADMIN"]}>
                <VetPage />
              </RequireRole>
            </RequireAuth>
          }
        />
        <Route
          path="/owner/pets"
          element={
            <RequireAuth>
              <RequireRole roles={["OWNER", "ADMIN"]}>
                <OwnerPetsPage />
              </RequireRole>
            </RequireAuth>
          }
        />
        <Route path="/owner/profile" element={<OwnerProfilePage />} />

        {/* Admin (parent shell + nested children) */}
        <Route
          path="/admin"
          element={
            <RequireAdmin>
              <AdminShell />
            </RequireAdmin>
          }
        >
          {/* index = /admin */}
          <Route index element={<AdminDashboard />} />

          {/* children */}
          <Route path="vets" element={<AdminVetListPage />} />
          <Route path="vets/new" element={<AdminAddVetPage />} />
          <Route path="vets/:id" element={<AdminVetDetailsPage />} />

          <Route path="users" element={<AdminUsersPage />} />
          <Route path="users/:id" element={<AdminUserEditPage />} />

          <Route path="pets" element={<AdminPetsPage />} />
          <Route path="pets/new" element={<AdminPetAddPage />} />
          <Route path="pets/:id/edit" element={<AdminPetEditPage />} />

          {/* future: appointments, inventory */}
          {/* <Route path="appointments" element={<AdminAppointmentsPage />} /> */}
          {/* <Route path="inventory" element={<AdminInventoryPage />} /> */}
        </Route>

        {/* Misc */}
        <Route path="/unauthorized" element={<Unauthorized />} />
        <Route path="*" element={<NotFound />} />
      </Routes>
    </Router>
  );
};

export default App;
