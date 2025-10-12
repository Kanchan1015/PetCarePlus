import React from 'react';
import { useParams } from 'react-router-dom';
import PetMedicalRecords from './PetMedicalRecords';

const PetMedicalRecordsWrapper: React.FC = () => {
  const { petId } = useParams<{ petId: string }>();
  
  if (!petId) {
    return <div>Pet ID is required</div>;
  }

  return <PetMedicalRecords petId={petId} petName="Pet" />;
};

export default PetMedicalRecordsWrapper;