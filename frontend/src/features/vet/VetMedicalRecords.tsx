import React, { useState, useEffect } from 'react';
import { Plus, Search, Filter } from 'lucide-react';
import { adminPetsApi } from '../shared/api/petsApi';
import PetMedicalRecords from '../owner/PetMedicalRecords';
import AddMedicalRecordForm from './AddMedicalRecordForm';
import type { Pet } from '../shared/types/pet';
import { getSpeciesLabel } from '../shared/types/pet';

const VetMedicalRecords: React.FC = () => {
  const [pets, setPets] = useState<Pet[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedPet, setSelectedPet] = useState<Pet | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const [showRecords, setShowRecords] = useState(false);

  useEffect(() => {
    loadPets();
  }, []);

  const loadPets = async () => {
    try {
      setLoading(true);
      const petsData = await adminPetsApi.getAll();
      
      // Convert PetSummary to Pet objects by fetching detailed data
      const detailedPets = await Promise.all(
        petsData.map(async (petSummary) => {
          try {
            return await adminPetsApi.getById(petSummary.id);
          } catch (err) {
            console.error(`Failed to load details for pet ${petSummary.id}:`, err);
            // Return a basic Pet object with available data
            return {
              id: petSummary.id,
              name: petSummary.name,
              species: petSummary.species,
              breed: petSummary.breed,
              isActive: petSummary.isActive,
              ownerUserId: '',
              ownerFullName: petSummary.ownerFullName,
              createdAt: new Date().toISOString(),
              ageInYears: petSummary.ageInYears
            } as Pet;
          }
        })
      );
      
      setPets(detailedPets);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load pets');
    } finally {
      setLoading(false);
    }
  };

  const handleAddRecord = (pet: Pet) => {
    setSelectedPet(pet);
    setShowAddForm(true);
    setShowRecords(false);
  };

  const handleViewRecords = (pet: Pet) => {
    setSelectedPet(pet);
    setShowRecords(true);
    setShowAddForm(false);
  };

  const handleFormSuccess = () => {
    setShowAddForm(false);
    setSelectedPet(null);
    // Optionally refresh the records view
  };

  const handleFormCancel = () => {
    setShowAddForm(false);
    setSelectedPet(null);
  };

  const handleBackToList = () => {
    setShowRecords(false);
    setSelectedPet(null);
  };

  const filteredPets = pets.filter(pet =>
    pet.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    pet.ownerFullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    getSpeciesLabel(pet.species).toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent mx-auto mb-4"></div>
          <div className="text-gray-600 font-medium">Loading pets...</div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 border-l-4 border-red-400 rounded-r-xl p-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center text-red-700">
            <div className="w-6 h-6 mr-3">⚠️</div>
            <div>
              <h3 className="font-medium">Error Loading Pets</h3>
              <p className="text-sm mt-1">{error}</p>
            </div>
          </div>
          <button
            onClick={loadPets}
            className="ml-4 text-red-600 hover:text-red-800 underline font-medium"
          >
            Try again
          </button>
        </div>
      </div>
    );
  }

  // Show add record form
  if (showAddForm && selectedPet) {
    return (
      <div className="space-y-6">
        <div className="flex items-center text-blue-600 hover:text-blue-800 mb-4">
          <button
            onClick={handleFormCancel}
            className="flex items-center font-medium"
          >
            ← Back to pets list
          </button>
        </div>
        <AddMedicalRecordForm
          petId={selectedPet.id}
          petName={selectedPet.name}
          onSuccess={handleFormSuccess}
          onCancel={handleFormCancel}
        />
      </div>
    );
  }

  // Show medical records
  if (showRecords && selectedPet) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <button
            onClick={handleBackToList}
            className="flex items-center text-blue-600 hover:text-blue-800 font-medium"
          >
            ← Back to pets list
          </button>
          <button
            onClick={() => handleAddRecord(selectedPet)}
            className="inline-flex items-center rounded-2xl bg-blue-600 text-white px-4 py-2 text-sm font-semibold hover:bg-blue-700 focus-visible:ring focus-visible:ring-blue-500 transition-colors"
          >
            <Plus className="w-4 h-4 mr-2" />
            Add Record
          </button>
        </div>
        <PetMedicalRecords petId={selectedPet.id} petName={selectedPet.name} />
      </div>
    );
  }

  // Show pets list
  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-3xl font-bold text-blue-800 mb-2">Medical Records Management</h2>
        <p className="text-gray-600 mb-6">
          Manage medical records for all pets 🏥📋
        </p>
      </div>

      {/* Search and Filters */}
      <div className="bg-white rounded-xl shadow-md p-6 border-l-4 border-blue-500">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search pets by name, owner, or species..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <button className="flex items-center px-4 py-2 text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200 transition-colors">
            <Filter className="w-4 h-4 mr-2" />
            Filters
          </button>
        </div>
      </div>

      {/* Pets Grid */}
      {filteredPets.length === 0 ? (
        <div className="bg-white rounded-xl shadow-md p-8 text-center border-l-4 border-gray-400">
          <p className="text-gray-500 text-lg mb-2">
            {searchTerm ? 'No pets found matching your search.' : 'No pets available.'}
          </p>
          {searchTerm && (
            <p className="text-gray-400 text-sm">
              Try adjusting your search criteria
            </p>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredPets.map((pet) => (
            <div key={pet.id} className="bg-white rounded-xl shadow-md p-6 border-l-4 border-blue-300 hover:shadow-lg transition-all duration-200 hover:border-blue-500">
              <div className="mb-4">
                <div className="flex items-start justify-between mb-2">
                  <h3 className="text-lg font-semibold text-blue-900">{pet.name}</h3>
                  <span
                    className={`inline-block text-xs px-2 py-1 rounded-full ${
                      pet.isActive
                        ? 'bg-green-100 text-green-700'
                        : 'bg-red-100 text-red-700'
                    }`}
                  >
                    {pet.isActive ? 'Active' : 'Inactive'}
                  </span>
                </div>
                <p className="text-gray-600 mb-1">{getSpeciesLabel(pet.species)} • {pet.breed || 'Mixed breed'}</p>
                {pet.ownerFullName && (
                  <p className="text-sm text-gray-500">Owner: {pet.ownerFullName}</p>
                )}
              </div>

              <div className="space-y-2 mb-4">
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="text-gray-500 font-medium">Age:</span>
                    <span className="ml-2 text-gray-700">{pet.ageInYears || 'Unknown'} years</span>
                  </div>
                  <div>
                    <span className="text-gray-500 font-medium">Species:</span>
                    <span className="ml-2 text-gray-700">{getSpeciesLabel(pet.species)}</span>
                  </div>
                  <div>
                    <span className="text-gray-500 font-medium">Weight:</span>
                    <span className="ml-2 text-gray-700">{pet.weight ? `${pet.weight} kg` : 'Not recorded'}</span>
                  </div>
                  <div>
                    <span className="text-gray-500 font-medium">Color:</span>
                    <span className="ml-2 text-gray-700">{pet.color || 'Not recorded'}</span>
                  </div>
                </div>
              </div>

              <div className="pt-4 border-t border-gray-100 flex gap-2">
                <button
                  onClick={() => handleViewRecords(pet)}
                  className="flex-1 px-3 py-2 text-blue-600 border border-blue-600 rounded-md hover:bg-blue-50 transition-colors text-sm font-medium"
                >
                  View Records
                </button>
                <button
                  onClick={() => handleAddRecord(pet)}
                  className="flex-1 px-3 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors text-sm font-medium flex items-center justify-center"
                >
                  <Plus className="w-4 h-4 mr-1" />
                  Add Record
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default VetMedicalRecords;