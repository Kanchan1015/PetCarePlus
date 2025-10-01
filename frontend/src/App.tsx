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
import AdminShell from "./features/admin/layout/AdminShell";
import VetShell from "./features/vet/layout/VetShell";
import OwnerShell from "./features/owner/layout/OwnerShell";
import PetMedicalRecordsWrapper from "./features/owner/PetMedicalRecordsWrapper";
import VetMedicalRecords from "./features/vet/VetMedicalRecords";

const App = () => {
  return (
    <Router>
      <Routes>
        {/* Public */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegistrationPage />} />
        <Route path="/" element={<LandingPage />} />

        {/* Owner Routes (parent shell + nested children) */}
        <Route
          path="/owner"
          element={
            <RequireAuth>
              <RequireRole roles={["OWNER", "ADMIN", "VET"]}>
                <OwnerShell />
              </RequireRole>
            </RequireAuth>
          }
        >
          {/* index = /owner */}
          <Route index element={<OwnerPage />} />
          
          {/* Owner nested routes */}
          <Route path="pets" element={<OwnerPetsPage />} />
          <Route path="pets/:petId/medical-records" element={<PetMedicalRecordsWrapper />} />
          <Route path="profile" element={<OwnerProfilePage />} />
        </Route>

        {/* Vet Routes (parent shell + nested children) */}
        <Route
          path="/vet"
          element={
            <RequireAuth>
              <RequireRole roles={["VET", "ADMIN"]}>
                <VetShell />
              </RequireRole>
            </RequireAuth>
          }
        >
          {/* index = /vet */}
          <Route index element={<VetPage />} />
          
          {/* Vet nested routes */}
          <Route path="medical-records" element={<VetMedicalRecords />} />
        </Route>

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
