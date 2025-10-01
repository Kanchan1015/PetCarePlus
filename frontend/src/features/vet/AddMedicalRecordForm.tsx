import React, { useState } from 'react';
import { Save, X } from 'lucide-react';
import { medicalRecordsApi, vaccinationsApi, treatmentsApi, prescriptionsApi } from '../shared/api/medicalRecordsApi';
import type { 
  CreateMedicalRecordRequest, 
  CreateVaccinationRequest, 
  CreateTreatmentRequest, 
  CreatePrescriptionRequest 
} from '../shared/types/medicalRecords';
import { MedicalRecordType, MEDICAL_RECORD_TYPE_LABELS } from '../shared/types/medicalRecords';

interface AddMedicalRecordFormProps {
  petId: string;
  petName: string;
  onSuccess: () => void;
  onCancel: () => void;
}

type RecordType = 'general' | 'vaccination' | 'treatment' | 'prescription';

const AddMedicalRecordForm: React.FC<AddMedicalRecordFormProps> = ({
  petId,
  petName,
  onSuccess,
  onCancel
}) => {
  const [recordType, setRecordType] = useState<RecordType>('general');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // General medical record form
  const [generalForm, setGeneralForm] = useState<CreateMedicalRecordRequest>({
    petId,
    recordType: MedicalRecordType.Other,
    recordDate: new Date().toISOString().split('T')[0],
    title: '',
    description: '',
    notes: ''
  });

  // Vaccination form
  const [vaccinationForm, setVaccinationForm] = useState<CreateVaccinationRequest>({
    petId,
    vaccineName: '',
    vaccinationDate: new Date().toISOString().split('T')[0],
    nextDueDate: '',
    batchNumber: '',
    manufacturer: '',
    notes: ''
  });

  // Treatment form
  const [treatmentForm, setTreatmentForm] = useState<CreateTreatmentRequest>({
    petId,
    treatmentType: '',
    diagnosis: '',
    treatmentDescription: '',
    treatmentDate: new Date().toISOString().split('T')[0],
    followUpDate: '',
    medications: '',
    instructions: '',
    notes: ''
  });

  // Prescription form
  const [prescriptionForm, setPrescriptionForm] = useState<CreatePrescriptionRequest>({
    petId,
    medicationName: '',
    dosage: '',
    frequency: '',
    prescribedDate: new Date().toISOString().split('T')[0],
    startDate: '',
    endDate: '',
    durationDays: undefined,
    instructions: '',
    notes: ''
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      let result;
      
      switch (recordType) {
        case 'general':
          result = await medicalRecordsApi.createMedicalRecord(generalForm);
          break;
        case 'vaccination':
          result = await vaccinationsApi.createVaccination(vaccinationForm);
          break;
        case 'treatment':
          result = await treatmentsApi.createTreatment(treatmentForm);
          break;
        case 'prescription':
          result = await prescriptionsApi.createPrescription(prescriptionForm);
          break;
        default:
          throw new Error('Invalid record type');
      }

      if (!result.ok) {
        throw new Error(result.message);
      }

      onSuccess();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create medical record');
    } finally {
      setLoading(false);
    }
  };

  const renderGeneralForm = () => (
    <div className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Record Type
        </label>
        <select
          value={generalForm.recordType}
          onChange={(e) => setGeneralForm(prev => ({ 
            ...prev, 
            recordType: Number(e.target.value) as MedicalRecordType 
          }))}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        >
          {Object.entries(MEDICAL_RECORD_TYPE_LABELS).map(([value, label]) => (
            <option key={value} value={value}>{label}</option>
          ))}
        </select>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Title *
        </label>
        <input
          type="text"
          value={generalForm.title}
          onChange={(e) => setGeneralForm(prev => ({ ...prev, title: e.target.value }))}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Record Date *
        </label>
        <input
          type="date"
          value={generalForm.recordDate}
          onChange={(e) => setGeneralForm(prev => ({ ...prev, recordDate: e.target.value }))}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Description
        </label>
        <textarea
          value={generalForm.description}
          onChange={(e) => setGeneralForm(prev => ({ ...prev, description: e.target.value }))}
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          value={generalForm.notes}
          onChange={(e) => setGeneralForm(prev => ({ ...prev, notes: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>
  );

  const renderVaccinationForm = () => (
    <div className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Vaccine Name *
        </label>
        <input
          type="text"
          value={vaccinationForm.vaccineName}
          onChange={(e) => setVaccinationForm(prev => ({ ...prev, vaccineName: e.target.value }))}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Vaccination Date *
          </label>
          <input
            type="date"
            value={vaccinationForm.vaccinationDate}
            onChange={(e) => setVaccinationForm(prev => ({ ...prev, vaccinationDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Next Due Date
          </label>
          <input
            type="date"
            value={vaccinationForm.nextDueDate}
            onChange={(e) => setVaccinationForm(prev => ({ ...prev, nextDueDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Batch Number
          </label>
          <input
            type="text"
            value={vaccinationForm.batchNumber}
            onChange={(e) => setVaccinationForm(prev => ({ ...prev, batchNumber: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Manufacturer
          </label>
          <input
            type="text"
            value={vaccinationForm.manufacturer}
            onChange={(e) => setVaccinationForm(prev => ({ ...prev, manufacturer: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          value={vaccinationForm.notes}
          onChange={(e) => setVaccinationForm(prev => ({ ...prev, notes: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>
  );

  const renderTreatmentForm = () => (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Treatment Type *
          </label>
          <input
            type="text"
            value={treatmentForm.treatmentType}
            onChange={(e) => setTreatmentForm(prev => ({ ...prev, treatmentType: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Treatment Date *
          </label>
          <input
            type="date"
            value={treatmentForm.treatmentDate}
            onChange={(e) => setTreatmentForm(prev => ({ ...prev, treatmentDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Diagnosis *
        </label>
        <textarea
          value={treatmentForm.diagnosis}
          onChange={(e) => setTreatmentForm(prev => ({ ...prev, diagnosis: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Treatment Description
        </label>
        <textarea
          value={treatmentForm.treatmentDescription}
          onChange={(e) => setTreatmentForm(prev => ({ ...prev, treatmentDescription: e.target.value }))}
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Medications
          </label>
          <textarea
            value={treatmentForm.medications}
            onChange={(e) => setTreatmentForm(prev => ({ ...prev, medications: e.target.value }))}
            rows={2}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Follow-up Date
          </label>
          <input
            type="date"
            value={treatmentForm.followUpDate}
            onChange={(e) => setTreatmentForm(prev => ({ ...prev, followUpDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Instructions
        </label>
        <textarea
          value={treatmentForm.instructions}
          onChange={(e) => setTreatmentForm(prev => ({ ...prev, instructions: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          value={treatmentForm.notes}
          onChange={(e) => setTreatmentForm(prev => ({ ...prev, notes: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>
  );

  const renderPrescriptionForm = () => (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Medication Name *
          </label>
          <input
            type="text"
            value={prescriptionForm.medicationName}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, medicationName: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Dosage *
          </label>
          <input
            type="text"
            value={prescriptionForm.dosage}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, dosage: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="e.g., 10mg"
            required
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Frequency *
          </label>
          <input
            type="text"
            value={prescriptionForm.frequency}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, frequency: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="e.g., Twice daily"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Prescribed Date *
          </label>
          <input
            type="date"
            value={prescriptionForm.prescribedDate}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, prescribedDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Start Date
          </label>
          <input
            type="date"
            value={prescriptionForm.startDate}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, startDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            End Date
          </label>
          <input
            type="date"
            value={prescriptionForm.endDate}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, endDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Duration (Days)
          </label>
          <input
            type="number"
            value={prescriptionForm.durationDays || ''}
            onChange={(e) => setPrescriptionForm(prev => ({ 
              ...prev, 
              durationDays: e.target.value ? Number(e.target.value) : undefined 
            }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            min="1"
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Instructions
        </label>
        <textarea
          value={prescriptionForm.instructions}
          onChange={(e) => setPrescriptionForm(prev => ({ ...prev, instructions: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="e.g., Take with food"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          value={prescriptionForm.notes}
          onChange={(e) => setPrescriptionForm(prev => ({ ...prev, notes: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>
  );

  return (
    <div className="bg-white rounded-xl shadow-md p-6 border-l-4 border-blue-500">
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-blue-800 mb-2">
          Add Medical Record - {petName}
        </h2>
        <p className="text-gray-600">Create a new medical record for this pet 📋✨</p>
      </div>

      {error && (
        <div className="mb-6 bg-red-50 border-l-4 border-red-400 p-4 rounded-r-lg">
          <p className="text-red-700 text-sm font-medium">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Record Type Selection */}
        <div className="mb-6">
          <label className="block text-sm font-semibold text-blue-800 mb-3">
            Select Record Type
          </label>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
            <button
              type="button"
              onClick={() => setRecordType('general')}
              className={`p-4 text-sm rounded-xl border-2 transition-all duration-200 ${
                recordType === 'general'
                  ? 'bg-blue-50 border-blue-300 text-blue-800 shadow-md'
                  : 'bg-white border-gray-200 text-gray-700 hover:bg-gray-50 hover:border-gray-300'
              }`}
            >
              <div className="font-medium">General Record</div>
              <div className="text-xs text-gray-500 mt-1">Basic medical info</div>
            </button>
            <button
              type="button"
              onClick={() => setRecordType('vaccination')}
              className={`p-4 text-sm rounded-xl border-2 transition-all duration-200 ${
                recordType === 'vaccination'
                  ? 'bg-blue-50 border-blue-300 text-blue-800 shadow-md'
                  : 'bg-white border-gray-200 text-gray-700 hover:bg-gray-50 hover:border-gray-300'
              }`}
            >
              <div className="font-medium">Vaccination</div>
              <div className="text-xs text-gray-500 mt-1">Vaccine records</div>
            </button>
            <button
              type="button"
              onClick={() => setRecordType('treatment')}
              className={`p-4 text-sm rounded-xl border-2 transition-all duration-200 ${
                recordType === 'treatment'
                  ? 'bg-blue-50 border-blue-300 text-blue-800 shadow-md'
                  : 'bg-white border-gray-200 text-gray-700 hover:bg-gray-50 hover:border-gray-300'
              }`}
            >
              <div className="font-medium">Treatment</div>
              <div className="text-xs text-gray-500 mt-1">Medical procedures</div>
            </button>
            <button
              type="button"
              onClick={() => setRecordType('prescription')}
              className={`p-4 text-sm rounded-xl border-2 transition-all duration-200 ${
                recordType === 'prescription'
                  ? 'bg-blue-50 border-blue-300 text-blue-800 shadow-md'
                  : 'bg-white border-gray-200 text-gray-700 hover:bg-gray-50 hover:border-gray-300'
              }`}
            >
              <div className="font-medium">Prescription</div>
              <div className="text-xs text-gray-500 mt-1">Medications</div>
            </button>
          </div>
        </div>

        {/* Form Fields */}
        {recordType === 'general' && renderGeneralForm()}
        {recordType === 'vaccination' && renderVaccinationForm()}
        {recordType === 'treatment' && renderTreatmentForm()}
        {recordType === 'prescription' && renderPrescriptionForm()}

        {/* Submit Buttons */}
        <div className="flex justify-end space-x-4 mt-8 pt-6 border-t border-gray-200">
          <button
            type="button"
            onClick={onCancel}
            className="inline-flex items-center rounded-2xl border border-gray-300 bg-white px-6 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus-visible:ring focus-visible:ring-gray-400 transition-colors"
          >
            <X className="w-4 h-4 mr-2" />
            Cancel
          </button>
          <button
            type="submit"
            disabled={loading}
            className="inline-flex items-center rounded-2xl bg-blue-600 px-6 py-2 text-sm font-semibold text-white hover:bg-blue-700 focus-visible:ring focus-visible:ring-blue-500 disabled:opacity-50 transition-colors"
          >
            <Save className="w-4 h-4 mr-2" />
            {loading ? 'Saving...' : 'Save Record'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default AddMedicalRecordForm;