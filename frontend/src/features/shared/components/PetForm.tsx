import React, { useState, useEffect } from 'react';
import { Species } from '../types/pet';
import type { CreatePetRequest, UpdatePetRequest } from '../types/pet';

interface PetFormProps {
  initialData?: {
    id?: string;
    name?: string;
    species?: Species;
    breed?: string;
    dateOfBirth?: string;
    color?: string;
    weight?: number;
    medicalNotes?: string;
    isActive?: boolean;
  };
  onSubmit: (data: CreatePetRequest | UpdatePetRequest) => void;
  onCancel: () => void;
  isLoading?: boolean;
  mode: 'create' | 'edit';
  ownerUserId?: string; // For create mode
}

export const PetForm: React.FC<PetFormProps> = ({
  initialData,
  onSubmit,
  onCancel,
  isLoading = false,
  mode,
  ownerUserId
}) => {
  const [formData, setFormData] = useState({
    name: initialData?.name || '',
    species: initialData?.species || ('' as Species | ''),
    breed: initialData?.breed || '',
    dateOfBirth: initialData?.dateOfBirth || '',
    color: initialData?.color || '',
    weight: initialData?.weight || 0,
    medicalNotes: initialData?.medicalNotes || '',
    isActive: initialData?.isActive ?? true
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (initialData) {
      setFormData({
        name: initialData.name || '',
        species: initialData.species || ('' as Species | ''),
        breed: initialData.breed || '',
        dateOfBirth: initialData.dateOfBirth || '',
        color: initialData.color || '',
        weight: initialData.weight || 0,
        medicalNotes: initialData.medicalNotes || '',
        isActive: initialData.isActive ?? true
      });
    }
  }, [initialData]);

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Pet name is required';
    }

    if (!formData.species) {
      newErrors.species = 'Species is required';
    }

    if (!formData.dateOfBirth) {
      newErrors.dateOfBirth = 'Date of birth is required';
    } else {
      const birthDate = new Date(formData.dateOfBirth);
      const today = new Date();
      if (birthDate > today) {
        newErrors.dateOfBirth = 'Date of birth cannot be in the future';
      }
    }

    if (formData.weight && formData.weight <= 0) {
      newErrors.weight = 'Weight must be greater than 0';
    }

    if (mode === 'create' && !ownerUserId) {
      newErrors.owner = 'Owner must be specified for new pets';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    const submitData = {
      name: formData.name,
      species: formData.species as Species,
      breed: formData.breed || undefined,
      dateOfBirth: formData.dateOfBirth || undefined,
      color: formData.color || undefined,
      weight: formData.weight || undefined,
      medicalNotes: formData.medicalNotes || undefined
    };

    if (mode === 'edit') {
      const updateData: UpdatePetRequest = {
        ...submitData,
        isActive: formData.isActive
      };
      onSubmit(updateData);
    } else {
      if (!ownerUserId) return;
      const createData: CreatePetRequest = {
        ...submitData,
        ownerUserId
      };
      onSubmit(createData);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: 
        name === 'weight' ? (value ? parseFloat(value) : 0) :
        name === 'species' ? (value ? parseInt(value) : '') :
        type === 'checkbox' ? (e.target as HTMLInputElement).checked :
        value
    }));

    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {/* Pet Name */}
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
          Pet Name *
        </label>
        <input
          type="text"
          id="name"
          name="name"
          value={formData.name}
          onChange={handleChange}
          className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.name ? 'border-red-500' : 'border-gray-300'
          }`}
          placeholder="Enter pet name"
        />
        {errors.name && <p className="mt-1 text-sm text-red-600">{errors.name}</p>}
      </div>

      {/* Species */}
      <div>
        <label htmlFor="species" className="block text-sm font-medium text-gray-700 mb-1">
          Species *
        </label>
        <select
          id="species"
          name="species"
          value={formData.species}
          onChange={handleChange}
          className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.species ? 'border-red-500' : 'border-gray-300'
          }`}
        >
          <option value="">Select species</option>
          {Object.entries(Species).map(([key, value]) => (
            <option key={key} value={value}>
              {key.replace('_', ' ')}
            </option>
          ))}
        </select>
        {errors.species && <p className="mt-1 text-sm text-red-600">{errors.species}</p>}
      </div>

      {/* Breed */}
      <div>
        <label htmlFor="breed" className="block text-sm font-medium text-gray-700 mb-1">
          Breed
        </label>
        <input
          type="text"
          id="breed"
          name="breed"
          value={formData.breed}
          onChange={handleChange}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Enter breed"
        />
      </div>

      {/* Date of Birth */}
      <div>
        <label htmlFor="dateOfBirth" className="block text-sm font-medium text-gray-700 mb-1">
          Date of Birth
        </label>
        <input
          type="date"
          id="dateOfBirth"
          name="dateOfBirth"
          value={formData.dateOfBirth}
          onChange={handleChange}
          className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.dateOfBirth ? 'border-red-500' : 'border-gray-300'
          }`}
        />
        {errors.dateOfBirth && <p className="mt-1 text-sm text-red-600">{errors.dateOfBirth}</p>}
      </div>

      {/* Color */}
      <div>
        <label htmlFor="color" className="block text-sm font-medium text-gray-700 mb-1">
          Color
        </label>
        <input
          type="text"
          id="color"
          name="color"
          value={formData.color}
          onChange={handleChange}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Enter color"
        />
      </div>

      {/* Weight */}
      <div>
        <label htmlFor="weight" className="block text-sm font-medium text-gray-700 mb-1">
          Weight (kg)
        </label>
        <input
          type="number"
          id="weight"
          name="weight"
          value={formData.weight || ''}
          onChange={handleChange}
          min="0"
          step="0.1"
          className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.weight ? 'border-red-500' : 'border-gray-300'
          }`}
          placeholder="Enter weight"
        />
        {errors.weight && <p className="mt-1 text-sm text-red-600">{errors.weight}</p>}
      </div>

      {/* Medical Notes */}
      <div>
        <label htmlFor="medicalNotes" className="block text-sm font-medium text-gray-700 mb-1">
          Medical Notes
        </label>
        <textarea
          id="medicalNotes"
          name="medicalNotes"
          value={formData.medicalNotes}
          onChange={handleChange}
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Enter any medical notes"
        />
      </div>

      {/* Active Status - Only show in edit mode */}
      {mode === 'edit' && (
        <div className="flex items-center">
          <input
            type="checkbox"
            id="isActive"
            name="isActive"
            checked={formData.isActive}
            onChange={handleChange}
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
          />
          <label htmlFor="isActive" className="ml-2 block text-sm text-gray-700">
            Pet is active
          </label>
        </div>
      )}

      {/* Action Buttons */}
      <div className="flex justify-end space-x-3 pt-4">
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 text-gray-700 bg-gray-200 rounded-md hover:bg-gray-300 focus:outline-none focus:ring-2 focus:ring-gray-500"
          disabled={isLoading}
        >
          Cancel
        </button>
        <button
          type="submit"
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
          disabled={isLoading}
        >
          {isLoading ? 'Saving...' : mode === 'create' ? 'Create Pet' : 'Update Pet'}
        </button>
      </div>
    </form>
  );
};