export const Species = {
  Dog: 1,
  Cat: 2,
  Bird: 3,
  Fish: 4,
  Rabbit: 5,
  Hamster: 6,
  Guinea_Pig: 7,
  Reptile: 8,
  Other: 99
} as const;

export type Species = typeof Species[keyof typeof Species];

export interface UserSelection {
  id: string;
  fullName: string;
  email: string;
  role: string;
}

export interface Pet {
  id: string;
  name: string;
  species: Species;
  breed?: string;
  dateOfBirth?: string;
  color?: string;
  weight?: number;
  medicalNotes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  ownerUserId: string;
  ownerFullName?: string;
  ageInYears?: number;
}

export interface PetSummary {
  id: string;
  name: string;
  species: Species;
  breed?: string;
  ageInYears?: number;
  isActive: boolean;
  ownerFullName: string;
}

export interface CreatePetRequest {
  name: string;
  species: Species;
  breed?: string;
  dateOfBirth?: string;
  color?: string;
  weight?: number;
  medicalNotes?: string;
  ownerUserId: string;
}

export interface UpdatePetRequest {
  name: string;
  species: Species;
  breed?: string;
  dateOfBirth?: string;
  color?: string;
  weight?: number;
  medicalNotes?: string;
  isActive: boolean;
}

export interface AssignPetRequest {
  petId: string;
  newOwnerUserId: string;
}

export const SPECIES_LABELS: Record<Species, string> = {
  [Species.Dog]: 'Dog',
  [Species.Cat]: 'Cat',
  [Species.Bird]: 'Bird',
  [Species.Fish]: 'Fish',
  [Species.Rabbit]: 'Rabbit',
  [Species.Hamster]: 'Hamster',
  [Species.Guinea_Pig]: 'Guinea Pig',
  [Species.Reptile]: 'Reptile',
  [Species.Other]: 'Other'
};

export const getSpeciesLabel = (species: Species): string => {
  return SPECIES_LABELS[species] || 'Unknown';
};