import React from 'react';
import { getSpeciesLabel } from '../types/pet';
import type { Pet, PetSummary } from '../types/pet';

interface PetCardProps {
  pet: Pet | PetSummary;
  showOwner?: boolean;
  onEdit?: () => void;
  onDelete?: () => void;
  onAssign?: () => void;
  onView?: () => void;
}

export const PetCard: React.FC<PetCardProps> = ({
  pet,
  showOwner = false,
  onEdit,
  onDelete,
  onAssign,
  onView
}) => {
  // Type guard to check if pet is a full Pet object
  const isFullPet = (pet: Pet | PetSummary): pet is Pet => {
    return 'createdAt' in pet && 'ownerUserId' in pet;
  };

  const fullPet = isFullPet(pet) ? pet : null;

  return (
    <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow">
      {/* Header */}
      <div className="flex justify-between items-start mb-4">
        <div>
          <h3 className="text-lg font-semibold text-gray-900">{pet.name}</h3>
          <p className="text-sm text-gray-600">{getSpeciesLabel(pet.species)}</p>
        </div>
        
        {/* Status Badge */}
        <span
          className={`px-2 py-1 text-xs font-medium rounded-full ${
            pet.isActive
              ? 'bg-green-100 text-green-800'
              : 'bg-red-100 text-red-800'
          }`}
        >
          {pet.isActive ? 'Active' : 'Inactive'}
        </span>
      </div>

      {/* Pet Details */}
      <div className="space-y-2 mb-4">
        {pet.breed && <p className="text-sm text-gray-700">Breed: {pet.breed}</p>}
        {pet.ageInYears !== undefined && (
          <p className="text-sm text-gray-700">Age: {pet.ageInYears} years old</p>
        )}
        
        {/* Show additional details only for full Pet objects */}
        {fullPet && (
          <>
            {fullPet.color && <p className="text-sm text-gray-700">Color: {fullPet.color}</p>}
            {fullPet.weight && <p className="text-sm text-gray-700">Weight: {fullPet.weight} kg</p>}
          </>
        )}

        {/* Medical Notes - only for full Pet objects */}
        {fullPet && fullPet.medicalNotes && (
          <div className="text-sm text-gray-700">
            <strong>Medical Notes:</strong> {fullPet.medicalNotes}
          </div>
        )}
      </div>

      {/* Owner Info */}
      {showOwner && pet.ownerFullName && (
        <div className="border-t pt-3 mb-4">
          <p className="text-sm text-gray-600">
            Owner: <span className="font-medium">{pet.ownerFullName}</span>
          </p>
        </div>
      )}

      {/* Action Buttons */}
      <div className="flex gap-2 flex-wrap">
        {onView && (
          <button
            onClick={onView}
            className="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition-colors"
          >
            View
          </button>
        )}
        
        {onEdit && (
          <button
            onClick={onEdit}
            className="px-3 py-1 text-sm bg-yellow-100 text-yellow-700 rounded hover:bg-yellow-200 transition-colors"
          >
            Edit
          </button>
        )}
        
        {onAssign && (
          <button
            onClick={onAssign}
            className="px-3 py-1 text-sm bg-purple-100 text-purple-700 rounded hover:bg-purple-200 transition-colors"
          >
            Assign
          </button>
        )}
        
        {onDelete && (
          <button
            onClick={onDelete}
            className="px-3 py-1 text-sm bg-red-100 text-red-700 rounded hover:bg-red-200 transition-colors"
          >
            Delete
          </button>
        )}
      </div>
    </div>
  );
};