import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { PetCard } from "../../shared/components";
import { petsApi } from "../../shared/api/petsApi";
import type { Pet } from "../../shared/types/pet";
import { debugToken } from "../../../utils/tokenDebug";

interface PetDetailsModalProps {
  isOpen: boolean;
  onClose: () => void;
  pet: Pet;
  onViewMedicalRecords: (pet: Pet) => void;
}

const PetDetailsModal: React.FC<PetDetailsModalProps> = ({
  isOpen,
  onClose,
  pet,
  onViewMedicalRecords,
}) => {
  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur"
      role="dialog"
      aria-modal="true"
    >
      <div className="w-full max-w-2xl max-h-[90vh] overflow-y-auto rounded-3xl bg-white ring-1 ring-slate-200 shadow-xl">
        <div className="p-6">
          <div className="mb-4 flex items-start justify-between">
            <h2 className="text-2xl font-bold tracking-tight text-slate-800">
              {pet.name}
            </h2>
            <button
              onClick={onClose}
              className="inline-flex h-10 w-10 items-center justify-center rounded-xl text-slate-500 hover:bg-slate-100 hover:text-slate-700 focus-visible:ring focus-visible:ring-indigo-500"
              aria-label="Close modal"
            >
              ×
            </button>
          </div>

          <div className="space-y-5">
            {/* Basic Info */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium uppercase tracking-wide text-slate-500">
                  Species
                </label>
                <p className="text-slate-800">{pet.species}</p>
              </div>
              {pet.breed && (
                <div>
                  <label className="block text-xs font-medium uppercase tracking-wide text-slate-500">
                    Breed
                  </label>
                  <p className="text-slate-800">{pet.breed}</p>
                </div>
              )}
            </div>

            <div className="grid grid-cols-2 gap-4">
              {pet.dateOfBirth && (
                <div>
                  <label className="block text-xs font-medium uppercase tracking-wide text-slate-500">
                    Date of Birth
                  </label>
                  <p className="text-slate-800">
                    {new Date(pet.dateOfBirth).toLocaleDateString()}
                  </p>
                </div>
              )}
              {pet.ageInYears !== undefined && (
                <div>
                  <label className="block text-xs font-medium uppercase tracking-wide text-slate-500">
                    Age
                  </label>
                  <p className="text-slate-800">{pet.ageInYears} years old</p>
                </div>
              )}
            </div>

            <div className="grid grid-cols-2 gap-4">
              {pet.color && (
                <div>
                  <label className="block text-xs font-medium uppercase tracking-wide text-slate-500">
                    Color
                  </label>
                  <p className="text-slate-800">{pet.color}</p>
                </div>
              )}
              {pet.weight && (
                <div>
                  <label className="block text-xs font-medium uppercase tracking-wide text-slate-500">
                    Weight
                  </label>
                  <p className="text-slate-800">{pet.weight} kg</p>
                </div>
              )}
            </div>

            {/* Medical Notes */}
            {pet.medicalNotes && (
              <div>
                <label className="mb-2 block text-xs font-medium uppercase tracking-wide text-slate-500">
                  Medical Notes
                </label>
                <div className="rounded-2xl bg-slate-50 p-3 ring-1 ring-slate-200">
                  <p className="whitespace-pre-wrap text-slate-800">
                    {pet.medicalNotes}
                  </p>
                </div>
              </div>
            )}

            {/* Timestamps */}
            <div className="pt-4">
              <div className="grid grid-cols-2 gap-4 text-sm text-slate-600">
                <div>
                  <label className="block text-xs font-medium uppercase tracking-wide text-slate-500">
                    Created
                  </label>
                  <p>{new Date(pet.createdAt).toLocaleDateString()}</p>
                </div>
                {pet.updatedAt && (
                  <div>
                    <label className="block text-xs font-medium uppercase tracking-wide text-slate-500">
                      Last Updated
                    </label>
                    <p>{new Date(pet.updatedAt).toLocaleDateString()}</p>
                  </div>
                )}
              </div>
            </div>
          </div>

          <div className="mt-6 flex justify-end space-x-3">
            <button
              onClick={() => onViewMedicalRecords(pet)}
              className="inline-flex items-center rounded-2xl bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 focus-visible:ring focus-visible:ring-blue-500"
            >
              View Medical Records
            </button>
            <button
              onClick={onClose}
              className="inline-flex items-center rounded-2xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 focus-visible:ring focus-visible:ring-indigo-500"
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export const OwnerPetsPage: React.FC = () => {
  const navigate = useNavigate();
  const [pets, setPets] = useState<Pet[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedPet, setSelectedPet] = useState<Pet | null>(null);
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);

  useEffect(() => {
    loadMyPets();
    debugToken();
  }, []);

  const loadMyPets = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const token = localStorage.getItem("APP_AT");
      console.log("Token check - Token exists:", !!token);
      if (token) console.log("Token preview:", token.substring(0, 50) + "...");

      try {
        console.log("Testing backend claims...");
        const debugInfo = await petsApi.debugMe();
        console.log("Backend debug info:", debugInfo);
      } catch (debugError) {
        console.error("Backend debug failed:", debugError);
      }

      const data = await petsApi.getMy();
      setPets(data);
    } catch (error) {
      console.error("Failed to load pets:", error);
      setError("Failed to load your pets. Please try again.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleViewDetails = (pet: Pet) => {
    setSelectedPet(pet);
    setIsDetailsModalOpen(true);
  };

  const handleViewMedicalRecords = (pet: Pet) => {
    navigate(`/owner/pets/${pet.id}/medical-records`);
  };

  const filteredPets = pets.filter(
    (pet) =>
      pet.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      pet.breed?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const activePets = filteredPets.filter((pet) => pet.isActive);
  const inactivePets = filteredPets.filter((pet) => !pet.isActive);

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="text-lg text-slate-600">Loading your pets...</div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight text-slate-800">
          My Pets
        </h1>
        <p className="mt-1 text-slate-600">
          Manage and view your pet&apos;s information
        </p>
      </div>

      {/* Search */}
      <div className="max-w-md">
        <input
          type="text"
          placeholder="Search your pets by name or breed..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full rounded-xl border-0 px-3 py-2 ring-1 ring-slate-200 text-slate-800 placeholder:text-slate-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500"
        />
      </div>

      {/* Error State */}
      {error && (
        <div className="rounded-2xl bg-red-50 p-4 text-red-800 ring-1 ring-red-200">
          <p>{error}</p>
          <button
            onClick={loadMyPets}
            className="mt-2 inline-flex items-center text-sm font-medium text-red-700 underline hover:text-red-800"
          >
            Try again
          </button>
        </div>
      )}

      {/* Active Pets */}
      {activePets.length > 0 && (
        <div>
          <h2 className="mb-4 text-lg font-semibold text-slate-800">
            Active Pets
          </h2>
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
            {activePets.map((pet) => (
              <PetCard
                key={pet.id}
                pet={pet}
                showOwner={false}
                onView={() => handleViewDetails(pet)}
                onViewMedicalRecords={() => handleViewMedicalRecords(pet)}
              />
            ))}
          </div>
        </div>
      )}

      {/* Inactive Pets */}
      {inactivePets.length > 0 && (
        <div>
          <h2 className="mb-4 text-lg font-semibold text-slate-800">
            Inactive Pets
          </h2>
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
            {inactivePets.map((pet) => (
              <PetCard
                key={pet.id}
                pet={pet}
                showOwner={false}
                onView={() => handleViewDetails(pet)}
                onViewMedicalRecords={() => handleViewMedicalRecords(pet)}
              />
            ))}
          </div>
        </div>
      )}

      {/* Empty State */}
      {pets.length === 0 && !isLoading && (
        <div className="rounded-2xl bg-white p-8 text-center ring-1 ring-slate-200">
          <div className="mb-3 text-6xl text-slate-400">🐾</div>
          <p className="text-lg text-slate-600">
            You don&apos;t have any pets yet.
          </p>
          <p className="text-slate-500">
            Contact your veterinarian to add your pets to the system.
          </p>
        </div>
      )}

      {/* No Search Results */}
      {filteredPets.length === 0 && pets.length > 0 && searchTerm && (
        <div className="rounded-2xl bg-white p-8 text-center ring-1 ring-slate-200">
          <p className="text-lg text-slate-600">
            No pets found matching &quot;{searchTerm}&quot;.
          </p>
        </div>
      )}

      {/* Pet Details Modal */}
      {selectedPet && (
        <PetDetailsModal
          isOpen={isDetailsModalOpen}
          onClose={() => {
            setIsDetailsModalOpen(false);
            setSelectedPet(null);
          }}
          pet={selectedPet}
          onViewMedicalRecords={handleViewMedicalRecords}
        />
      )}
    </div>
  );
};
