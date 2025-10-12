import React, { useState } from 'react';
import { Download, FileText, Calendar } from 'lucide-react';
import { medicalRecordsApi, vaccinationsApi, treatmentsApi } from '../shared/api/medicalRecordsApi';
import { MedicalRecordsPDFService } from './services/pdfService';
import type { 
  TreatmentHistoryReport, 
  VaccinationReport 
} from '../shared/types/medicalRecords';
import type { Species } from '../shared/types/pet';

interface MedicalRecordsPDFExportProps {
  petId: string;
  petName: string;
  petSpecies: Species;
  ownerName?: string;
}

const MedicalRecordsPDFExport: React.FC<MedicalRecordsPDFExportProps> = ({
  petId,
  petName,
  petSpecies,
  ownerName
}) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleExportFullReport = async () => {
    setLoading(true);
    setError(null);

    try {
      // Fetch all medical records data
      const [recordsResult] = await Promise.all([
        medicalRecordsApi.getMedicalRecordsByPet(petId)
      ]);

      if (!recordsResult.ok) {
        throw new Error('Failed to fetch medical records data');
      }

      const records = recordsResult.data || [];
      // For vaccinations and treatments, we'll fetch them through the report APIs
      const vaccinationReport = await vaccinationsApi.getVaccinationReport(petId);
      const treatmentReport = await treatmentsApi.getTreatmentHistoryReport(petId);

      const vaccinations = vaccinationReport.ok ? vaccinationReport.data.vaccinations : [];
      const treatments = treatmentReport.ok ? treatmentReport.data.treatments : [];

      const pdfService = new MedicalRecordsPDFService();
      const petInfo = {
        name: petName,
        species: petSpecies,
        ownerName
      };

      await pdfService.generateFullReport(petInfo, records, vaccinations, treatments);
      const filename = `${petName}_Medical_Records_${new Date().toISOString().split('T')[0]}.pdf`;
      
      pdfService.download(filename);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to generate PDF report');
    } finally {
      setLoading(false);
    }
  };

  const handleExportVaccinationReport = async () => {
    setLoading(true);
    setError(null);

    try {
      const result = await vaccinationsApi.getVaccinationReport(petId);
      
      if (!result.ok) {
        throw new Error(result.message);
      }

      const reportData = result.data as VaccinationReport;
      
      const pdfService = new MedicalRecordsPDFService();
      const petInfo = {
        name: petName,
        species: petSpecies,
        ownerName
      };

      await pdfService.generateVaccinationReport(petInfo, reportData);
      const filename = `${petName}_Vaccination_Report_${new Date().toISOString().split('T')[0]}.pdf`;
      pdfService.download(filename);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to generate vaccination PDF report');
    } finally {
      setLoading(false);
    }
  };

  const handleExportTreatmentHistory = async () => {
    setLoading(true);
    setError(null);

    try {
      const result = await treatmentsApi.getTreatmentHistoryReport(petId);
      
      if (!result.ok) {
        throw new Error(result.message);
      }

      const reportData = result.data as TreatmentHistoryReport;
      
      const pdfService = new MedicalRecordsPDFService();
      const petInfo = {
        name: petName,
        species: petSpecies,
        ownerName
      };

      await pdfService.generateTreatmentReport(petInfo, reportData);
      const filename = `${petName}_Treatment_History_${new Date().toISOString().split('T')[0]}.pdf`;
      pdfService.download(filename);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to generate treatment history PDF report');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white border rounded-lg p-6">
      <div className="mb-6">
        <h3 className="text-lg font-semibold text-gray-800 flex items-center">
          <FileText className="w-5 h-5 mr-2" />
          Export Medical Reports as PDF
        </h3>
        <p className="text-gray-600 mt-1">Generate and download professional PDF reports with PetCare+ branding</p>
      </div>

      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 rounded p-3">
          <p className="text-red-700 text-sm">{error}</p>
        </div>
      )}

      <div className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <button
            onClick={handleExportFullReport}
            disabled={loading}
            className="flex items-center justify-center px-4 py-3 bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors disabled:opacity-50"
          >
            <Download className="w-4 h-4 mr-2" />
            {loading ? 'Generating...' : 'Full Medical Report'}
          </button>

          <button
            onClick={handleExportVaccinationReport}
            disabled={loading}
            className="flex items-center justify-center px-4 py-3 bg-green-600 text-white rounded hover:bg-green-700 transition-colors disabled:opacity-50"
          >
            <Calendar className="w-4 h-4 mr-2" />
            {loading ? 'Generating...' : 'Vaccination Report'}
          </button>

          <button
            onClick={handleExportTreatmentHistory}
            disabled={loading}
            className="flex items-center justify-center px-4 py-3 bg-purple-600 text-white rounded hover:bg-purple-700 transition-colors disabled:opacity-50"
          >
            <FileText className="w-4 h-4 mr-2" />
            {loading ? 'Generating...' : 'Treatment History'}
          </button>
        </div>

        <div className="text-sm text-gray-500 bg-gray-50 p-3 rounded">
          <p className="font-medium mb-1">PDF Report Information:</p>
          <ul className="list-disc list-inside space-y-1">
            <li><strong>Full Medical Report:</strong> Professional PDF with PetCare+ branding containing complete medical history</li>
            <li><strong>Vaccination Report:</strong> Formatted PDF with vaccination schedule, due dates, and recommendations</li>
            <li><strong>Treatment History:</strong> Comprehensive PDF timeline with diagnoses, treatments, and outcomes</li>
          </ul>
          <p className="mt-2 text-xs text-blue-600">
            ✨ All reports include the PetCare+ logo and professional formatting!
          </p>
        </div>
      </div>
    </div>
  );
};

export default MedicalRecordsPDFExport;