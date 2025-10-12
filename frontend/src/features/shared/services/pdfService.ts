import jsPDF from 'jspdf';
import type { 
  MedicalRecord, 
  Vaccination, 
  Treatment, 
  VaccinationReport, 
  TreatmentHistoryReport 
} from '../types/medicalRecords';
import { getSpeciesLabel, type Species } from '../types/pet';

interface PetInfo {
  name: string;
  species: Species;
  ownerName?: string;
}

export class MedicalRecordsPDFService {
  private pdf: jsPDF;
  private pageHeight: number;
  private currentY: number;
  private margin: number;
  private lineHeight: number;

  constructor() {
    this.pdf = new jsPDF();
    this.pageHeight = this.pdf.internal.pageSize.height;
    this.margin = 20;
    this.lineHeight = 7;
    this.currentY = this.margin;
  }

  private async addLogo(): Promise<void> {
    try {
      // Try to load and add the real logo image
      const logoDataUrl = await this.loadLogoImage();
      if (logoDataUrl) {
        this.pdf.addImage(logoDataUrl, 'JPEG', this.margin, 10, 40, 40);
        // Add PetCare+ text centered and bigger
        this.pdf.setTextColor(0, 0, 0); // Black text
        this.pdf.setFontSize(24);
        this.pdf.setFont('helvetica', 'bold');
        // Center the text
        const pageWidth = this.pdf.internal.pageSize.width;
        const textWidth = this.pdf.getTextWidth('PetCare+');
        const centerX = (pageWidth - textWidth) / 2;
        this.pdf.text('PetCare+', centerX, 35);
        this.currentY = 60;
      } else {
        this.addFallbackLogo();
      }
    } catch (error) {
      console.warn('Could not add logo to PDF:', error);
      this.addFallbackLogo();
    }
  }

  private loadLogoImage(): Promise<string | null> {
    return new Promise((resolve) => {
      const logoImg = new Image();
      logoImg.crossOrigin = 'anonymous';
      
      logoImg.onload = () => {
        try {
          const canvas = document.createElement('canvas');
          const ctx = canvas.getContext('2d');
          if (ctx) {
            canvas.width = logoImg.width;
            canvas.height = logoImg.height;
            ctx.drawImage(logoImg, 0, 0);
            resolve(canvas.toDataURL('image/jpeg'));
          } else {
            resolve(null);
          }
        } catch (err) {
          console.warn('Error converting image to data URL:', err);
          resolve(null);
        }
      };
      
      logoImg.onerror = () => {
        console.warn('Could not load logo image');
        resolve(null);
      };
      
      // Load the logo from public folder
      logoImg.src = '/logo.jpg';
    });
  }

  private addFallbackLogo(): void {
    // Fallback logo using graphics if image loading fails
    this.pdf.setFillColor(59, 130, 246); // Blue color
    this.pdf.circle(35, 25, 12, 'F');
    
    // Add paw print (simplified)
    this.pdf.setFillColor(255, 255, 255); // White
    this.pdf.ellipse(35, 28, 3, 4, 'F');
    this.pdf.ellipse(32, 22, 1.5, 2, 'F');
    this.pdf.ellipse(35, 20, 1.5, 2, 'F');
    this.pdf.ellipse(38, 22, 1.5, 2, 'F');
    
    // Add plus sign
    this.pdf.setFillColor(16, 185, 129); // Green
    this.pdf.rect(48, 21, 2, 8, 'F');
    this.pdf.rect(44, 24, 8, 2, 'F');
    
    // Add PetCare+ text centered and bigger
    this.pdf.setTextColor(0, 0, 0); // Black text
    this.pdf.setFontSize(24);
    this.pdf.setFont('helvetica', 'bold');
    // Center the text
    const pageWidth = this.pdf.internal.pageSize.width;
    const textWidth = this.pdf.getTextWidth('PetCare+');
    const centerX = (pageWidth - textWidth) / 2;
    this.pdf.text('PetCare+', centerX, 35);
    
    this.currentY = 60;
  }

  private async addHeader(title: string): Promise<void> {
    await this.addLogo();
    
    // Add report title
    this.pdf.setTextColor(0, 0, 0);
    this.pdf.setFontSize(18);
    this.pdf.setFont('helvetica', 'bold');
    this.pdf.text(title, this.margin, this.currentY);
    this.currentY += 15;

    // Add generation date
    this.pdf.setFontSize(10);
    this.pdf.setFont('helvetica', 'normal');
    this.pdf.setTextColor(100, 100, 100);
    const currentDate = new Date().toLocaleDateString();
    this.pdf.text(`Generated: ${currentDate}`, this.margin, this.currentY);
    this.currentY += 15;

    // Add separator line
    this.pdf.setDrawColor(200, 200, 200);
    this.pdf.line(this.margin, this.currentY, this.pdf.internal.pageSize.width - this.margin, this.currentY);
    this.currentY += 10;
  }

  private addPetInfo(petInfo: PetInfo): void {
    this.pdf.setFontSize(14);
    this.pdf.setFont('helvetica', 'bold');
    this.pdf.setTextColor(59, 130, 246); // Blue
    this.pdf.text('Pet Information', this.margin, this.currentY);
    this.currentY += 10;

    this.pdf.setFontSize(11);
    this.pdf.setFont('helvetica', 'normal');
    this.pdf.setTextColor(0, 0, 0);

    const info = [
      { label: 'Name:', value: petInfo.name },
      { label: 'Species:', value: getSpeciesLabel(petInfo.species) },
      ...(petInfo.ownerName ? [{ label: 'Owner:', value: petInfo.ownerName }] : [])
    ];

    info.forEach(({ label, value }) => {
      this.pdf.setFont('helvetica', 'bold');
      this.pdf.text(label, this.margin, this.currentY);
      this.pdf.setFont('helvetica', 'normal');
      this.pdf.text(value, this.margin + 25, this.currentY);
      this.currentY += this.lineHeight;
    });

    this.currentY += 5;
  }

  private checkPageBreak(requiredSpace: number = 20): void {
    if (this.currentY + requiredSpace > this.pageHeight - this.margin) {
      this.pdf.addPage();
      this.currentY = this.margin;
    }
  }

  private addSection(title: string, count?: number): void {
    this.checkPageBreak(15);
    
    this.pdf.setFillColor(240, 248, 255); // Light blue background
    this.pdf.rect(this.margin - 5, this.currentY - 5, this.pdf.internal.pageSize.width - 2 * this.margin + 10, 12, 'F');
    
    this.pdf.setFontSize(12);
    this.pdf.setFont('helvetica', 'bold');
    this.pdf.setTextColor(59, 130, 246); // Blue
    const sectionTitle = count !== undefined ? `${title} (${count})` : title;
    this.pdf.text(sectionTitle, this.margin, this.currentY + 3);
    this.currentY += 15;
  }

  private addRecord(record: MedicalRecord): void {
    this.checkPageBreak(25);

    // Record title
    this.pdf.setFontSize(11);
    this.pdf.setFont('helvetica', 'bold');
    this.pdf.setTextColor(0, 0, 0);
    this.pdf.text(`• ${record.title}`, this.margin + 5, this.currentY);
    this.currentY += this.lineHeight;

    // Record details
    this.pdf.setFont('helvetica', 'normal');
    this.pdf.setTextColor(80, 80, 80);
    
    const details = [
      `Date: ${new Date(record.recordDate).toLocaleDateString()}`,
      `Type: ${record.recordType}`,
      ...(record.description ? [`Description: ${record.description}`] : []),
      ...(record.notes ? [`Notes: ${record.notes}`] : [])
    ];

    details.forEach(detail => {
      this.pdf.text(`   ${detail}`, this.margin + 10, this.currentY);
      this.currentY += this.lineHeight;
    });

    this.currentY += 3;
  }

  private addVaccination(vaccination: Vaccination): void {
    this.checkPageBreak(25);

    // Vaccination name
    this.pdf.setFontSize(11);
    this.pdf.setFont('helvetica', 'bold');
    this.pdf.setTextColor(0, 0, 0);
    this.pdf.text(`• ${vaccination.vaccineName}`, this.margin + 5, this.currentY);
    this.currentY += this.lineHeight;

    // Vaccination details
    this.pdf.setFont('helvetica', 'normal');
    this.pdf.setTextColor(80, 80, 80);
    
    const details = [
      `Date: ${new Date(vaccination.vaccinationDate).toLocaleDateString()}`,
      ...(vaccination.nextDueDate ? [`Next Due: ${new Date(vaccination.nextDueDate).toLocaleDateString()}`] : []),
      ...(vaccination.manufacturer ? [`Manufacturer: ${vaccination.manufacturer}`] : []),
      ...(vaccination.batchNumber ? [`Batch: ${vaccination.batchNumber}`] : []),
      ...(vaccination.notes ? [`Notes: ${vaccination.notes}`] : [])
    ];

    details.forEach(detail => {
      this.pdf.text(`   ${detail}`, this.margin + 10, this.currentY);
      this.currentY += this.lineHeight;
    });

    this.currentY += 3;
  }

  private addTreatment(treatment: Treatment): void {
    this.checkPageBreak(30);

    // Treatment type
    this.pdf.setFontSize(11);
    this.pdf.setFont('helvetica', 'bold');
    this.pdf.setTextColor(0, 0, 0);
    this.pdf.text(`• ${treatment.treatmentType}`, this.margin + 5, this.currentY);
    this.currentY += this.lineHeight;

    // Treatment details
    this.pdf.setFont('helvetica', 'normal');
    this.pdf.setTextColor(80, 80, 80);
    
    const details = [
      `Date: ${new Date(treatment.treatmentDate).toLocaleDateString()}`,
      `Diagnosis: ${treatment.diagnosis}`,
      ...(treatment.treatmentDescription ? [`Treatment: ${treatment.treatmentDescription}`] : []),
      ...(treatment.medications ? [`Medications: ${treatment.medications}`] : []),
      ...(treatment.followUpDate ? [`Follow-up: ${new Date(treatment.followUpDate).toLocaleDateString()}`] : []),
      ...(treatment.instructions ? [`Instructions: ${treatment.instructions}`] : []),
      ...(treatment.notes ? [`Notes: ${treatment.notes}`] : [])
    ];

    details.forEach(detail => {
      this.pdf.text(`   ${detail}`, this.margin + 10, this.currentY);
      this.currentY += this.lineHeight;
    });

    this.currentY += 3;
  }

  private addFooter(): void {
    const totalPages = (this.pdf as any).internal.getNumberOfPages();
    
    for (let i = 1; i <= totalPages; i++) {
      this.pdf.setPage(i);
      
      // Add page number
      this.pdf.setFontSize(8);
      this.pdf.setFont('helvetica', 'normal');
      this.pdf.setTextColor(150, 150, 150);
      this.pdf.text(
        `Page ${i} of ${totalPages}`,
        this.pdf.internal.pageSize.width - this.margin - 20,
        this.pageHeight - 10
      );

      // Add footer text
      this.pdf.text(
        'Generated by PetCare+ Medical Records System',
        this.margin,
        this.pageHeight - 10
      );
    }
  }

  async generateFullReport(
    petInfo: PetInfo,
    records: MedicalRecord[],
    vaccinations: Vaccination[],
    treatments: Treatment[]
  ): Promise<void> {
    await this.addHeader('Complete Medical Records Report');
    this.addPetInfo(petInfo);

    // Add medical records section
    if (records.length > 0) {
      this.addSection('General Medical Records', records.length);
      records.forEach(record => this.addRecord(record));
    }

    // Add vaccinations section
    if (vaccinations.length > 0) {
      this.addSection('Vaccination Records', vaccinations.length);
      vaccinations.forEach(vaccination => this.addVaccination(vaccination));
    }

    // Add treatments section
    if (treatments.length > 0) {
      this.addSection('Treatment Records', treatments.length);
      treatments.forEach(treatment => this.addTreatment(treatment));
    }

    // Add no records message if everything is empty
    if (records.length === 0 && vaccinations.length === 0 && treatments.length === 0) {
      this.pdf.setFontSize(12);
      this.pdf.setFont('helvetica', 'normal');
      this.pdf.setTextColor(100, 100, 100);
      this.pdf.text(`No medical records found for ${petInfo.name}.`, this.margin, this.currentY);
    }

    this.addFooter();
  }

  async generateVaccinationReport(petInfo: PetInfo, vaccinationReport: VaccinationReport): Promise<void> {
    await this.addHeader('Vaccination Report');
    this.addPetInfo(petInfo);

    // Add vaccination summary
    this.addSection('Vaccination Summary');
    
    this.pdf.setFontSize(11);
    this.pdf.setFont('helvetica', 'normal');
    this.pdf.setTextColor(0, 0, 0);

    const summary = [
      `Total Vaccinations: ${vaccinationReport.vaccinations?.length || 0}`,
      `Overdue Vaccinations: ${vaccinationReport.overdueVaccinations?.length || 0}`,
      `Upcoming Vaccinations: ${vaccinationReport.upcomingVaccinations?.length || 0}`,
      `Up to Date: ${vaccinationReport.isUpToDate ? 'Yes' : 'No'}`
    ];

    summary.forEach(item => {
      this.pdf.text(`• ${item}`, this.margin + 5, this.currentY);
      this.currentY += this.lineHeight;
    });

    this.currentY += 5;

    // Add vaccination details
    if (vaccinationReport.vaccinations && vaccinationReport.vaccinations.length > 0) {
      this.addSection('Vaccination Details', vaccinationReport.vaccinations.length);
      vaccinationReport.vaccinations.forEach(vaccination => this.addVaccination(vaccination));
    }

    this.addFooter();
  }

  async generateTreatmentReport(petInfo: PetInfo, treatmentReport: TreatmentHistoryReport): Promise<void> {
    await this.addHeader('Treatment History Report');
    this.addPetInfo(petInfo);

    // Add treatment summary
    this.addSection('Treatment Summary');
    
    this.pdf.setFontSize(11);
    this.pdf.setFont('helvetica', 'normal');
    this.pdf.setTextColor(0, 0, 0);

    const summary = [
      `Total Treatments: ${treatmentReport.totalTreatments}`,
      ...(treatmentReport.lastTreatmentDate ? 
        [`Last Treatment: ${new Date(treatmentReport.lastTreatmentDate).toLocaleDateString()}`] : 
        ['Last Treatment: None'])
    ];

    summary.forEach(item => {
      this.pdf.text(`• ${item}`, this.margin + 5, this.currentY);
      this.currentY += this.lineHeight;
    });

    this.currentY += 5;

    // Add treatment details
    if (treatmentReport.treatments && treatmentReport.treatments.length > 0) {
      this.addSection('Treatment Details', treatmentReport.treatments.length);
      treatmentReport.treatments.forEach(treatment => this.addTreatment(treatment));
    }

    this.addFooter();
  }

  download(filename: string): void {
    this.pdf.save(filename);
  }

  getPDF(): jsPDF {
    return this.pdf;
  }
}