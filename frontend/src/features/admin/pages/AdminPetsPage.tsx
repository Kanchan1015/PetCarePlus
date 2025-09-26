import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PetCard } from '../../shared/components';
import { adminPetsApi } from '../../shared/api/petsApi';
import type { PetSummary } from '../../shared/types/pet';

export const AdminPetsPage: React.FC = () => {
  const navigate = useNavigate();
  const [pets, setPets] = useState<PetSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    loadPets();
  }, []);

  const loadPets = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await adminPetsApi.getAll();
      setPets(data);
    } catch (error) {
      console.error('Failed to load pets:', error);
      setError('Failed to load pets. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (pet: PetSummary) => {
    if (!confirm(`Are you sure you want to delete ${pet.name}? This action cannot be undone.`)) {
      return;
    }

    try {
      await adminPetsApi.delete(pet.id);
      await loadPets(); // Reload the list
    } catch (error) {
      console.error('Failed to delete pet:', error);
      alert('Failed to delete pet. Please try again.');
    }
  };

  const handleEdit = (pet: PetSummary) => {
    navigate(`/admin/pets/${pet.id}/edit`);
  };

  const handleAssign = async (pet: PetSummary) => {
    try {
      // Fetch full pet details for assignment
      const fullPet = await adminPetsApi.getById(pet.id);
      if (fullPet) {
        // You can implement assign logic here or create a separate assign page
        alert('Assign functionality can be implemented as needed');
      }
    } catch (error) {
      console.error('Failed to load pet details:', error);
      alert('Failed to load pet details. Please try again.');
    }
  };

  const handleCreatePet = () => {
    navigate('/admin/pets/new');
  };

  const filteredPets = pets.filter(pet =>
    pet.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    pet.breed?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    pet.ownerFullName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-lg text-gray-600">Loading pets...</div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-900">Pet Management</h1>
        <button
          onClick={handleCreatePet}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          Create New Pet
        </button>
      </div>

      {/* Search */}
      <div className="max-w-md">
        <input
          type="text"
          placeholder="Search pets by name, breed, or owner..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      {/* Error State */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-md p-4">
          <p className="text-red-800">{error}</p>
          <button
            onClick={loadPets}
            className="mt-2 text-red-600 hover:text-red-800 underline"
          >
            Try again
          </button>
        </div>
      )}

      {/* Pets Grid */}
      {filteredPets.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">
            {searchTerm ? 'No pets found matching your search.' : 'No pets found.'}
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredPets.map((pet) => (
            <PetCard
              key={pet.id}
              pet={pet}
              showOwner={true}
              onEdit={() => handleEdit(pet)}
              onDelete={() => handleDelete(pet)}
              onAssign={() => handleAssign(pet)}
            />
          ))}
        </div>
      )}
    </div>
  );
};