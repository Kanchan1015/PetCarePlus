import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { FileText, Calendar, Stethoscope, Plus, Eye } from 'lucide-react';
import { petsApi } from '../../shared/api/petsApi';
import { medicalRecordsApi, vaccinationsApi, treatmentsApi } from '../../shared/api/medicalRecordsApi';
import type { Pet } from '../../shared/types/pet';
import { getSpeciesLabel } from '../../shared/types/pet';

interface PetWithMedicalData extends Pet {
  medicalData?: {
    recordsCount: number;
    vaccinesCount: number;
    treatmentsCount: number;
    loading: boolean;
  };
}

const OwnerMedicalRecordsPage: React.FC = () => {
  const [pets, setPets] = useState<PetWithMedicalData[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadMyPets();
  }, []);

  const loadMyPets = async () => {
    try {
      setLoading(true);
      const petsData = await petsApi.getMy();
      
      // Initialize pets with loading medical data
      const petsWithMedicalData: PetWithMedicalData[] = petsData.map(pet => ({
        ...pet,
        medicalData: {
          recordsCount: 0,
          vaccinesCount: 0,
          treatmentsCount: 0,
          loading: true
        }
      }));
      
      setPets(petsWithMedicalData);
      
      // Load medical data for each pet
      await loadMedicalDataForPets(petsWithMedicalData);
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load pets');
    } finally {
      setLoading(false);
    }
  };

  const loadMedicalDataForPets = async (petsData: PetWithMedicalData[]) => {
    // Load medical data for each pet concurrently
    const promises = petsData.map(async (pet) => {
      try {
        console.log(`Loading medical data for pet: ${pet.name} (ID: ${pet.id})`);
        
        const [medicalRecordsResult, vaccinationResult, treatmentResult] = await Promise.all([
          medicalRecordsApi.getMedicalRecordsByPet(pet.id),
          vaccinationsApi.getVaccinationReport(pet.id),
          treatmentsApi.getTreatmentHistoryReport(pet.id)
        ]);

        console.log(`Results for ${pet.name}:`, {
          medicalRecords: medicalRecordsResult,
          vaccinations: vaccinationResult,
          treatments: treatmentResult
        });

        const medicalData = {
          recordsCount: medicalRecordsResult.ok ? medicalRecordsResult.data.length : 0,
          vaccinesCount: vaccinationResult.ok ? vaccinationResult.data.vaccinations.length : 0,
          treatmentsCount: treatmentResult.ok ? treatmentResult.data.totalTreatments : 0,
          loading: false
        };

        console.log(`Final medical data for ${pet.name}:`, medicalData);

        return {
          ...pet,
          medicalData
        };
      } catch (error) {
        console.error(`Failed to load medical data for pet ${pet.name}:`, error);
        return {
          ...pet,
          medicalData: {
            recordsCount: 0,
            vaccinesCount: 0,
            treatmentsCount: 0,
            loading: false
          }
        };
      }
    });

    const updatedPets = await Promise.all(promises);
    console.log('Final updated pets with medical data:', updatedPets);
    setPets(updatedPets);
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent mx-auto mb-4"></div>
          <div className="text-gray-600 font-medium">Loading your pets...</div>
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
            onClick={loadMyPets}
            className="ml-4 text-red-600 hover:text-red-800 underline font-medium"
          >
            Try again
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white rounded-xl shadow-md p-6 border-l-4 border-blue-500">
        <h1 className="text-3xl font-bold text-blue-800 mb-2">
          Pet Medical Records
        </h1>
        <p className="text-gray-600">
          View and manage medical records for all your pets 🏥📋
        </p>
      </div>

      {/* Pets Medical Records Grid */}
      {pets.length === 0 ? (
        <div className="bg-white rounded-xl shadow-md p-8 text-center border-l-4 border-gray-400">
          <div className="text-6xl text-gray-400 mb-4">🐾</div>
          <h2 className="text-xl font-semibold text-gray-700 mb-2">No Pets Found</h2>
          <p className="text-gray-500 mb-4">
            You don't have any pets registered yet.
          </p>
          <Link
            to="/owner/pets"
            className="inline-flex items-center px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors"
          >
            <Plus className="w-4 h-4 mr-2" />
            Add Your First Pet
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {pets.map((pet) => (
            <div
              key={pet.id}
              className="bg-white rounded-xl shadow-md p-6 border-l-4 border-blue-300 hover:shadow-lg transition-all duration-200 hover:border-blue-500"
            >
              {/* Pet Info Header */}
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
                <p className="text-gray-600 text-sm">
                  {getSpeciesLabel(pet.species)} • {pet.breed || 'Mixed breed'}
                </p>
              </div>

              {/* Pet Details */}
              <div className="space-y-2 mb-4">
                <div className="grid grid-cols-2 gap-2 text-sm">
                  <div>
                    <span className="text-gray-500 font-medium">Age:</span>
                    <span className="ml-2 text-gray-700">{pet.ageInYears || 'Unknown'} years</span>
                  </div>
                  <div>
                    <span className="text-gray-500 font-medium">Weight:</span>
                    <span className="ml-2 text-gray-700">{pet.weight ? `${pet.weight} kg` : 'Not set'}</span>
                  </div>
                  <div className="col-span-2">
                    <span className="text-gray-500 font-medium">Color:</span>
                    <span className="ml-2 text-gray-700">{pet.color || 'Not specified'}</span>
                  </div>
                </div>
              </div>

              {/* Medical Records Quick Stats */}
              <div className="bg-blue-50 rounded-lg p-3 mb-4">
                <h4 className="text-sm font-medium text-blue-800 mb-2 flex items-center">
                  <Stethoscope className="w-4 h-4 mr-1" />
                  Medical Overview
                  {pet.medicalData?.loading && (
                    <div className="ml-2 animate-spin rounded-full h-3 w-3 border-2 border-blue-500 border-t-transparent"></div>
                  )}
                </h4>
                <div className="grid grid-cols-3 gap-2 text-center">
                  <div className="bg-white rounded p-2">
                    <div className="text-sm font-semibold text-blue-700">
                      {pet.medicalData?.loading ? (
                        <div className="animate-spin rounded-full h-3 w-3 border-2 border-blue-500 border-t-transparent mx-auto"></div>
                      ) : (
                        pet.medicalData?.recordsCount || 0
                      )}
                    </div>
                    <div className="text-xs text-gray-600">Records</div>
                  </div>
                  <div className="bg-white rounded p-2">
                    <div className="text-sm font-semibold text-green-700">
                      {pet.medicalData?.loading ? (
                        <div className="animate-spin rounded-full h-3 w-3 border-2 border-green-500 border-t-transparent mx-auto"></div>
                      ) : (
                        pet.medicalData?.vaccinesCount || 0
                      )}
                    </div>
                    <div className="text-xs text-gray-600">Vaccines</div>
                  </div>
                  <div className="bg-white rounded p-2">
                    <div className="text-sm font-semibold text-purple-700">
                      {pet.medicalData?.loading ? (
                        <div className="animate-spin rounded-full h-3 w-3 border-2 border-purple-500 border-t-transparent mx-auto"></div>
                      ) : (
                        pet.medicalData?.treatmentsCount || 0
                      )}
                    </div>
                    <div className="text-xs text-gray-600">Treatments</div>
                  </div>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex gap-2">
                <Link
                  to={`/owner/pets/${pet.id}/medical-records`}
                  className="flex-1 flex items-center justify-center px-3 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm font-medium"
                >
                  <Eye className="w-4 h-4 mr-1" />
                  View Records
                  {!pet.medicalData?.loading && pet.medicalData && (
                    (pet.medicalData.recordsCount + pet.medicalData.vaccinesCount + pet.medicalData.treatmentsCount) > 0
                  ) && (
                    <span className="ml-1 bg-white bg-opacity-20 text-xs px-1.5 py-0.5 rounded-full">
                      {(pet.medicalData?.recordsCount || 0) + (pet.medicalData?.vaccinesCount || 0) + (pet.medicalData?.treatmentsCount || 0)}
                    </span>
                  )}
                </Link>
                <Link
                  to={`/owner/pets/${pet.id}/medical-records`}
                  className="flex items-center justify-center px-3 py-2 border border-blue-600 text-blue-600 rounded-lg hover:bg-blue-50 transition-colors text-sm font-medium"
                >
                  <FileText className="w-4 h-4" />
                </Link>
              </div>

              {/* Last Visit Info */}
              {pet.createdAt && (
                <div className="mt-3 pt-3 border-t border-gray-100">
                  <div className="flex items-center text-xs text-gray-500">
                    <Calendar className="w-3 h-3 mr-1" />
                    Registered: {new Date(pet.createdAt).toLocaleDateString()}
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Quick Actions */}
      {pets.length > 0 && (
        <div className="bg-white rounded-xl shadow-md p-6">
          <h3 className="text-lg font-semibold text-gray-800 mb-4">Quick Actions</h3>
          <div className="flex flex-wrap gap-3">
            <Link
              to="/owner/appointments/request"
              className="inline-flex items-center px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
            >
              <Calendar className="w-4 h-4 mr-2" />
              Book Appointment
            </Link>
            <button
              onClick={loadMyPets}
              disabled={loading}
              className="inline-flex items-center px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
            >
              <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              Refresh Data
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default OwnerMedicalRecordsPage;