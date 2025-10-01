import React, { useState } from 'react';
import { Download, FileText, Calendar } from 'lucide-react';
import { medicalRecordsApi, vaccinationsApi, treatmentsApi } from '../shared/api/medicalRecordsApi';
import type { 
  MedicalRecord, 
  Vaccination, 
  Treatment, 
  TreatmentHistoryReport, 
  VaccinationReport 
} from '../shared/types/medicalRecords';
import { getSpeciesLabel, type Species } from '../shared/types/pet';

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

  const generateTextReport = (
    records: MedicalRecord[], 
    vaccinations: Vaccination[], 
    treatments: Treatment[]
  ): string => {
    const currentDate = new Date().toLocaleDateString();
    
    let report = `MEDICAL RECORDS REPORT\n`;
    report += `Generated: ${currentDate}\n\n`;
    report += `PET INFORMATION\n`;
    report += `Name: ${petName}\n`;
    report += `Species: ${getSpeciesLabel(petSpecies)}\n`;
    if (ownerName) {
      report += `Owner: ${ownerName}\n`;
    }
    report += `\n${'='.repeat(50)}\n\n`;

    // General Medical Records
    if (records.length > 0) {
      report += `GENERAL MEDICAL RECORDS (${records.length})\n`;
      report += `${'='.repeat(30)}\n\n`;
      
      records.forEach((record, index) => {
        report += `${index + 1}. ${record.title}\n`;
        report += `   Date: ${new Date(record.recordDate).toLocaleDateString()}\n`;
        report += `   Type: ${record.recordType}\n`;
        if (record.description) {
          report += `   Description: ${record.description}\n`;
        }
        if (record.notes) {
          report += `   Notes: ${record.notes}\n`;
        }
        report += `\n`;
      });
    }

    // Vaccinations
    if (vaccinations.length > 0) {
      report += `VACCINATION RECORDS (${vaccinations.length})\n`;
      report += `${'='.repeat(30)}\n\n`;
      
      vaccinations.forEach((vaccination, index) => {
        report += `${index + 1}. ${vaccination.vaccineName}\n`;
        report += `   Date: ${new Date(vaccination.vaccinationDate).toLocaleDateString()}\n`;
        if (vaccination.nextDueDate) {
          report += `   Next Due: ${new Date(vaccination.nextDueDate).toLocaleDateString()}\n`;
        }
        if (vaccination.manufacturer) {
          report += `   Manufacturer: ${vaccination.manufacturer}\n`;
        }
        if (vaccination.batchNumber) {
          report += `   Batch: ${vaccination.batchNumber}\n`;
        }
        if (vaccination.notes) {
          report += `   Notes: ${vaccination.notes}\n`;
        }
        report += `\n`;
      });
    }

    // Treatments
    if (treatments.length > 0) {
      report += `TREATMENT RECORDS (${treatments.length})\n`;
      report += `${'='.repeat(30)}\n\n`;
      
      treatments.forEach((treatment, index) => {
        report += `${index + 1}. ${treatment.treatmentType}\n`;
        report += `   Date: ${new Date(treatment.treatmentDate).toLocaleDateString()}\n`;
        report += `   Diagnosis: ${treatment.diagnosis}\n`;
        if (treatment.treatmentDescription) {
          report += `   Treatment: ${treatment.treatmentDescription}\n`;
        }
        if (treatment.medications) {
          report += `   Medications: ${treatment.medications}\n`;
        }
        if (treatment.followUpDate) {
          report += `   Follow-up: ${new Date(treatment.followUpDate).toLocaleDateString()}\n`;
        }
        if (treatment.instructions) {
          report += `   Instructions: ${treatment.instructions}\n`;
        }
        if (treatment.notes) {
          report += `   Notes: ${treatment.notes}\n`;
        }
        report += `\n`;
      });
    }

    if (records.length === 0 && vaccinations.length === 0 && treatments.length === 0) {
      report += `No medical records found for ${petName}.\n`;
    }

    report += `\n${'='.repeat(50)}\n`;
    report += `End of Report\n`;

    return report;
  };

  const downloadTextFile = (content: string, filename: string) => {
    const blob = new Blob([content], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  const handleExportFullReport = async () => {
    setLoading(true);
    setError(null);

    try {
      // Fetch all medical records data - for this demo, we'll use the available API methods
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

      const reportContent = generateTextReport(records, vaccinations, treatments);
      const filename = `${petName}_Medical_Records_${new Date().toISOString().split('T')[0]}.txt`;
      
      downloadTextFile(reportContent, filename);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to generate report');
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
      
      let reportContent = `VACCINATION REPORT\n`;
      reportContent += `Generated: ${new Date().toLocaleDateString()}\n\n`;
      reportContent += `Pet: ${petName}\n`;
      reportContent += `Species: ${getSpeciesLabel(petSpecies)}\n`;
      if (ownerName) {
        reportContent += `Owner: ${ownerName}\n`;
      }
      reportContent += `\n${'='.repeat(40)}\n\n`;
      
      reportContent += `VACCINATION SUMMARY\n`;
      reportContent += `Total Vaccinations: ${reportData.vaccinations?.length || 0}\n`;
      reportContent += `Overdue Vaccinations: ${reportData.overdueVaccinations?.length || 0}\n`;
      reportContent += `Upcoming Vaccinations: ${reportData.upcomingVaccinations?.length || 0}\n`;
      reportContent += `Up to Date: ${reportData.isUpToDate ? 'Yes' : 'No'}\n\n`;

      if (reportData.vaccinations && reportData.vaccinations.length > 0) {
        reportContent += `VACCINATION DETAILS\n`;
        reportContent += `${'='.repeat(20)}\n\n`;
        
        reportData.vaccinations.forEach((vaccination, index) => {
          reportContent += `${index + 1}. ${vaccination.vaccineName}\n`;
          reportContent += `   Date: ${new Date(vaccination.vaccinationDate).toLocaleDateString()}\n`;
          if (vaccination.nextDueDate) {
            reportContent += `   Next Due: ${new Date(vaccination.nextDueDate).toLocaleDateString()}\n`;
          }
          if (vaccination.manufacturer) {
            reportContent += `   Manufacturer: ${vaccination.manufacturer}\n`;
          }
          reportContent += `\n`;
        });
      }

      const filename = `${petName}_Vaccination_Report_${new Date().toISOString().split('T')[0]}.txt`;
      downloadTextFile(reportContent, filename);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to generate vaccination report');
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
      
      let reportContent = `TREATMENT HISTORY REPORT\n`;
      reportContent += `Generated: ${new Date().toLocaleDateString()}\n\n`;
      reportContent += `Pet: ${petName}\n`;
      reportContent += `Species: ${getSpeciesLabel(petSpecies)}\n`;
      if (ownerName) {
        reportContent += `Owner: ${ownerName}\n`;
      }
      reportContent += `\n${'='.repeat(40)}\n\n`;
      
      reportContent += `TREATMENT SUMMARY\n`;
      reportContent += `Total Treatments: ${reportData.totalTreatments}\n`;
      if (reportData.lastTreatmentDate) {
        reportContent += `Last Treatment: ${new Date(reportData.lastTreatmentDate).toLocaleDateString()}\n`;
      }
      reportContent += `\n`;

      if (reportData.treatments && reportData.treatments.length > 0) {
        reportContent += `TREATMENT DETAILS\n`;
        reportContent += `${'='.repeat(20)}\n\n`;
        
        reportData.treatments.forEach((treatment, index) => {
          reportContent += `${index + 1}. ${treatment.treatmentType}\n`;
          reportContent += `   Date: ${new Date(treatment.treatmentDate).toLocaleDateString()}\n`;
          reportContent += `   Diagnosis: ${treatment.diagnosis}\n`;
          if (treatment.treatmentDescription) {
            reportContent += `   Treatment: ${treatment.treatmentDescription}\n`;
          }
          if (treatment.medications) {
            reportContent += `   Medications: ${treatment.medications}\n`;
          }
          reportContent += `\n`;
        });
      }

      const filename = `${petName}_Treatment_History_${new Date().toISOString().split('T')[0]}.txt`;
      downloadTextFile(reportContent, filename);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to generate treatment history report');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white border rounded-lg p-6">
      <div className="mb-6">
        <h3 className="text-lg font-semibold text-gray-800 flex items-center">
          <FileText className="w-5 h-5 mr-2" />
          Export Medical Reports
        </h3>
        <p className="text-gray-600 mt-1">Generate and download comprehensive medical reports</p>
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
          <p className="font-medium mb-1">Report Information:</p>
          <ul className="list-disc list-inside space-y-1">
            <li><strong>Full Medical Report:</strong> Complete medical history including all records, vaccinations, and treatments</li>
            <li><strong>Vaccination Report:</strong> Detailed vaccination schedule with due dates and recommendations</li>
            <li><strong>Treatment History:</strong> Comprehensive treatment timeline with diagnoses and outcomes</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default MedicalRecordsPDFExport;