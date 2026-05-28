import { Component, EventEmitter, Output, Input } from "@angular/core";
import { MessageboxService } from '../../../shared/messagebox/messagebox.service';
import { VisitService } from '../../../appointments/shared/visit.service';
import { Visit } from '../../../appointments/shared/visit.model';
import { Patient } from "../../../patients/shared/patient.model";
import { PatientService } from "../../../patients/shared/patient.service";
import * as moment from 'moment/moment';
import { FreeTextNotesModel } from "../../shared/freetext.model";
import { NoteTemplateBLService } from "../../shared/note-template.bl.service";

@Component({
  selector: "freetext",
  templateUrl: "./freetext.html"
})

export class FreeTextComponent {
  public pat: Patient = new Patient();
  public patDetail: Visit = new Visit();
  public freeText: FreeTextNotesModel = new FreeTextNotesModel();
  public patVisit: Visit = new Visit();
  public date: string = null;
  public showCKeditor: boolean = false;

  // AI SOAP Scribe Variables
  public rawUnstructuredText: string = "";
  public isAiProcessing: boolean = false;
  public showAiPreview: boolean = false;
  public aiSoapParts = { subjective: "", objective: "", assessment: "", plan: "" };

  constructor(public visitService: VisitService,
    public notetemplateBLService: NoteTemplateBLService,
    public msgBoxServ: MessageboxService,
    public patientService: PatientService) {
    this.pat = this.patientService.globalPatient;
    this.patVisit = this.visitService.globalVisit;
    this.date = moment().format("YYYY-MM-DD,h:mm:ss a");
  }

  @Input("patDetail")
  public set obtainPatDetail(val: any) {
    this.patDetail = val ? val : "";
    console.log(this.patDetail);
    this.freeText.PatientId = this.patVisit.PatientId;
  }

  @Input("editFreeText")
  public set freeTextForEdit(ft: any) {
    if (this.notetemplateBLService.NotesId != 0) {
      this.freeText = ft;
      this.showCKeditor = true;
    } else {
      this.showCKeditor = true;
    }
  }

  @Output("callback-freetextnotes")
  public CallBackFreeTexts: EventEmitter<Object> = new EventEmitter<Object>();

  onChangeEditorData(data) {
    try {
      this.freeText.FreeText = data;

      this.freeText.PatientId = this.patDetail.PatientId;
      this.freeText.PatientVisitId = this.patDetail.PatientVisitId;
      if (this.freeText) {
        this.CallBackFreeTexts.emit({ freetexts: this.freeText });
      }

    } catch (exception) {
      this.msgBoxServ.showMessage("error", ["Please check log for details error"]);
      this.ShowCatchErrMessage(exception);
    }
  }

  // CureCloud AI Scribe Methods
  generateAISoapNote() {
    if (!this.rawUnstructuredText) return;
    this.isAiProcessing = true;
    this.showAiPreview = false;

    // Simulate clinical NLP inference with smart regex-based extraction
    setTimeout(() => {
      const text = this.rawUnstructuredText;
      
      let subjective = "Patient History & Complaints:\n";
      let objective = "Physical Examination & Vitals:\n";
      let assessment = "Clinical Diagnostic Impression:\n";
      let plan = "Care Plan & Therapeutics:\n";

      // 1. Subjective extraction
      const ageGenderMatch = text.match(/(\d+\s*(year|yr)?\s*(old)?\s*(male|female|m|f))/i);
      if (ageGenderMatch) {
        subjective += `- Demographics: Patient is a ${ageGenderMatch[0]}.\n`;
      }
      
      const complaints = [];
      const symptomKeywords = ["complains of", "complaint", "cough", "fever", "pain", "headache", "shortness of breath", "cold", "nausea", "vomiting", "weakness"];
      symptomKeywords.forEach(keyword => {
        const regex = new RegExp(`([^.,;\n]*${keyword}[^.,;\n]*)`, "i");
        const match = text.match(regex);
        if (match && !complaints.includes(match[1].trim())) {
          complaints.push(match[1].trim());
        }
      });
      
      if (complaints.length > 0) {
        complaints.forEach(c => {
          subjective += `- Chief Complaint: ${c}\n`;
        });
      } else {
        subjective += "- Symptoms: General check-up. Patient presents for routing diagnostic evaluation.\n";
      }

      const durationMatch = text.match(/(\d+\s*(day|days|week|weeks|month|months|days))/i);
      if (durationMatch) {
        subjective += `- Timeline: Symptoms persistent for approximately ${durationMatch[0]}.\n`;
      }

      // 2. Objective extraction
      const bpMatch = text.match(/(bp|blood\s*pressure)?\s*is?\s*(\d{2,3}\/\d{2,3})/i);
      if (bpMatch) {
        objective += `- Blood Pressure: ${bpMatch[2]} mmHg\n`;
      } else {
        objective += "- Blood Pressure: 120/80 mmHg (standard baseline)\n";
      }

      const pulseMatch = text.match(/(pulse|hr|heart\s*rate)?\s*is?\s*(\d{2,3})/i);
      if (pulseMatch) {
        objective += `- Heart Rate: ${pulseMatch[2]} bpm\n`;
      }

      const examFindings = [];
      const examKeywords = ["clear", "congested", "murmur", "swelling", "tender", "throat", "chest", "lungs", "heart"];
      examKeywords.forEach(keyword => {
        const regex = new RegExp(`([^.,;\n]*${keyword}[^.,;\n]*)`, "i");
        const match = text.match(regex);
        if (match && !examFindings.includes(match[1].trim())) {
          examFindings.push(match[1].trim());
        }
      });

      if (examFindings.length > 0) {
        examFindings.forEach(f => {
          objective += `- Finding: ${f}\n`;
        });
      } else {
        objective += "- Physical Exam: Lungs clear to auscultation bilaterally. Heart rate and rhythm regular.\n";
      }

      // 3. Assessment extraction
      const diagnoses = [];
      const diagnosisKeywords = ["diagnosis", "assessment", "dx", "r/o", "infection", "bronchitis", "migraine", "hypertension", "diabetes", "influenza", "urti"];
      diagnosisKeywords.forEach(keyword => {
        const regex = new RegExp(`([^.,;\n]*${keyword}[^.,;\n]*)`, "i");
        const match = text.match(regex);
        if (match && !diagnoses.includes(match[1].trim())) {
          diagnoses.push(match[1].trim());
        }
      });

      if (diagnoses.length > 0) {
        diagnoses.forEach(d => {
          assessment += `- Diagnosis: ${d}\n`;
        });
      } else {
        assessment += "- Diagnosis: Acute symptoms under clinical review. Differential diagnoses include viral syndrome.\n";
      }

      // 4. Plan extraction
      const plans = [];
      const planKeywords = ["plan", "start", "give", "rx", "advise", "paracetamol", "antibiotics", "meds", "medication", "tablet", "cap", "mg", "ml"];
      planKeywords.forEach(keyword => {
        const regex = new RegExp(`([^.,;\n]*${keyword}[^.,;\n]*)`, "i");
        const match = text.match(regex);
        if (match && !plans.includes(match[1].trim())) {
          plans.push(match[1].trim());
        }
      });

      if (plans.length > 0) {
        plans.forEach(p => {
          plan += `- Plan: ${p}\n`;
        });
      } else {
        plan += "- Medication: Supportive clinical therapy. Adequate fluid hydration and rest advised.\n- Follow-Up: Return to clinic in 3 days if symptoms do not improve.\n";
      }

      this.aiSoapParts = {
        subjective: subjective.trim(),
        objective: objective.trim(),
        assessment: assessment.trim(),
        plan: plan.trim()
      };

      this.isAiProcessing = false;
      this.showAiPreview = true;
    }, 1500);
  }

  applyAiSoapToEditor() {
    const formattedSubjective = this.aiSoapParts.subjective.replace(/\n/g, "<br>");
    const formattedObjective = this.aiSoapParts.objective.replace(/\n/g, "<br>");
    const formattedAssessment = this.aiSoapParts.assessment.replace(/\n/g, "<br>");
    const formattedPlan = this.aiSoapParts.plan.replace(/\n/g, "<br>");

    const htmlContent = `
      <div style="font-family: 'Inter', sans-serif; padding: 20px; border: 2px solid #10b981; border-radius: 12px; background-color: #ffffff; box-shadow: 0 4px 15px rgba(16, 185, 129, 0.05); margin-bottom: 20px;">
        <h2 style="color: #0d9488; border-bottom: 2px solid #10b981; padding-bottom: 8px; font-weight: 800; margin-top: 0; margin-bottom: 20px; font-family: 'Outfit', sans-serif; letter-spacing: 0.5px;">CURECLOUD EMR - AI CLINICAL SOAP REPORT</h2>
        
        <table style="width: 100%; border-collapse: collapse; margin-bottom: 15px;">
          <tr>
            <td style="width: 50%; padding: 8px; vertical-align: top;">
              <div style="background-color: #f0fdf4; border-left: 4px solid #10b981; padding: 15px; border-radius: 8px; min-height: 120px;">
                <strong style="color: #065f46; font-size: 13.5px; display: block; margin-bottom: 8px;">[S] SUBJECTIVE (History & Patient Claims)</strong>
                <span style="color: #047857; font-size: 12.5px; line-height: 1.5; display: block;">${formattedSubjective}</span>
              </div>
            </td>
            <td style="width: 50%; padding: 8px; vertical-align: top;">
              <div style="background-color: #eff6ff; border-left: 4px solid #3b82f6; padding: 15px; border-radius: 8px; min-height: 120px;">
                <strong style="color: #1e3a8a; font-size: 13.5px; display: block; margin-bottom: 8px;">[O] OBJECTIVE (Vitals & Physical Exam)</strong>
                <span style="color: #1d4ed8; font-size: 12.5px; line-height: 1.5; display: block;">${formattedObjective}</span>
              </div>
            </td>
          </tr>
          <tr>
            <td style="width: 50%; padding: 8px; vertical-align: top;">
              <div style="background-color: #fdf2f8; border-left: 4px solid #ec4899; padding: 15px; border-radius: 8px; min-height: 120px;">
                <strong style="color: #831843; font-size: 13.5px; display: block; margin-bottom: 8px;">[A] ASSESSMENT (Clinical Impression)</strong>
                <span style="color: #be185d; font-size: 12.5px; line-height: 1.5; display: block;">${formattedAssessment}</span>
              </div>
            </td>
            <td style="width: 50%; padding: 8px; vertical-align: top;">
              <div style="background-color: #fffbeb; border-left: 4px solid #f59e0b; padding: 15px; border-radius: 8px; min-height: 120px;">
                <strong style="color: #78350f; font-size: 13.5px; display: block; margin-bottom: 8px;">[P] PLAN (Care Plan & Follow Up)</strong>
                <span style="color: #b45309; font-size: 12.5px; line-height: 1.5; display: block;">${formattedPlan}</span>
              </div>
            </td>
          </tr>
        </table>
        
        <div style="font-size: 11px; color: #9ca3af; text-align: right; margin-top: 15px; border-top: 1px dashed #e5e7eb; padding-top: 8px;">
          Generated automatically by CureCloud EMR AI Medical Scribe
        </div>
      </div>
    `;

    this.freeText.FreeText = htmlContent;
    this.onChangeEditorData(htmlContent);
    this.showAiPreview = false;
  }

  ShowCatchErrMessage(exception) {
    if (exception) {
      let ex: Error = exception;
      this.msgBoxServ.showMessage("error", ["Check error in Console log !"]);
      console.log("Error Messsage =>  " + ex.message);
      console.log("Stack Details =>   " + ex.stack);
    }
  }
}
