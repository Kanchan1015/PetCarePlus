import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { PetForm } from '../../shared/components';
import { adminPetsApi } from '../../shared/api/petsApi';
import Breadcrumbs from '../../../components/Breadcrumbs';
import type { Pet, CreatePetRequest, UpdatePetRequest } from '../../shared/types/pet';

export const AdminPetEditPage: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [isLoading, setIsLoading] = useState(false);
  const [pet, setPet] = useState<Pet | null>(null);
  const [loadingPet, setLoadingPet] = useState(true);

  useEffect(() => {
    if (id) {
      loadPet(id);
    }
  }, [id]);

  const loadPet = async (petId: string) => {
    try {
      setLoadingPet(true);
      const petData = await adminPetsApi.getById(petId);
      if (petData) {
        setPet(petData);
      } else {
        alert('Pet not found');
        navigate('/admin/pets');
      }
    } catch (error) {
      console.error('Failed to load pet:', error);
      alert('Failed to load pet. Please try again.');
      navigate('/admin/pets');
    } finally {
      setLoadingPet(false);
    }
  };

  const handleSubmit = async (data: CreatePetRequest | UpdatePetRequest) => {
    if (!id || !pet) return;

    setIsLoading(true);
    try {
      await adminPetsApi.update(id, data as UpdatePetRequest);
      alert('Pet updated successfully!');
      navigate('/admin/pets');
    } catch (error) {
      console.error('Failed to update pet:', error);
      alert('Failed to update pet. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCancel = () => {
    navigate('/admin/pets');
  };

  const breadcrumbItems = [
    { label: 'Dashboard', to: '/admin' },
    { label: 'Pet Management', to: '/admin/pets' },
    { label: pet ? `Edit ${pet.name}` : 'Edit Pet' }
  ];

  if (loadingPet) {
    return (
      <div className="space-y-6">
        <Breadcrumbs items={breadcrumbItems} />
        <div className="flex items-center justify-center h-64">
          <div className="text-lg text-gray-600">Loading pet details...</div>
        </div>
      </div>
    );
  }

  if (!pet) {
    return (
      <div className="space-y-6">
        <Breadcrumbs items={breadcrumbItems} />
        <div className="bg-red-50 border border-red-200 rounded-md p-4">
          <p className="text-red-800">Pet not found.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <Breadcrumbs items={breadcrumbItems} />
      
      <div className="bg-white rounded-lg shadow-sm border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-200">
          <h1 className="text-2xl font-bold text-gray-900">Edit Pet: {pet.name}</h1>
          <p className="text-gray-600 mt-1">Update the pet details below</p>
          <div className="mt-2 text-sm text-gray-500">
            Owner: <span className="font-medium">{pet.ownerFullName}</span>
          </div>
        </div>
        
        <div className="p-6">
          <div className="max-w-2xl">
            <PetForm
              mode="edit"
              initialData={pet}
              onSubmit={handleSubmit}
              onCancel={handleCancel}
              isLoading={isLoading}
            />
          </div>
        </div>
      </div>
    </div>
  );
};