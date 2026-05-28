// ========================================
// Modern Card Component
// Material Design Healthcare Theme
// ========================================

import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-card',
  template: `
    <mat-card 
      [class.card-elevated]="elevated"
      [class.card-flat]="!elevated"
      class="modern-card">
      <mat-card-header *ngIf="title">
        <h3>{{ title }}</h3>
      </mat-card-header>
      <mat-card-content>
        <ng-content></ng-content>
      </mat-card-content>
      <mat-card-actions *ngIf="showActions" align="end">
        <ng-content select=".card-actions"></ng-content>
      </mat-card-actions>
    </mat-card>
  `,
  styles: [`
    .modern-card {
      border-radius: 12px;
      transition: all 250ms cubic-bezier(0.4, 0, 0.2, 1);
      border: 1px solid rgba(0, 0, 0, 0.05);
      
      &:hover {
        transform: translateY(-4px);
      }
    }

    .card-elevated {
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.12);
    }

    .card-flat {
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.08);
    }

    mat-card-header {
      margin-bottom: 16px;
      padding-bottom: 16px;
      border-bottom: 2px solid #f0f0f0;

      h3 {
        margin: 0;
        color: #1B7A5E;
        font-weight: 600;
        font-size: 18px;
      }
    }

    mat-card-content {
      padding: 0;
    }

    mat-card-actions {
      padding-top: 16px;
      margin-top: 16px;
      border-top: 1px solid #f0f0f0;
    }
  `]
})
export class ModernCardComponent {
  @Input() title: string = '';
  @Input() elevated: boolean = true;
  @Input() showActions: boolean = false;
}
