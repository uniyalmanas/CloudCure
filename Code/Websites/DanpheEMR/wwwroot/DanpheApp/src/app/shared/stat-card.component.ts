// ========================================
// Stat Card Component
// Material Design Healthcare Theme
// ========================================

import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-stat-card',
  template: `
    <div class="stat-card" [class]="'color-' + color">
      <div class="stat-icon">
        <mat-icon [color]="color">{{ icon }}</mat-icon>
      </div>
      <div class="stat-info">
        <div class="stat-value">{{ value }}</div>
        <div class="stat-title">{{ title }}</div>
      </div>
      <div class="stat-badge" *ngIf="trend">
        <mat-icon [class]="'trend-' + trend">
          {{ trend === 'up' ? 'trending_up' : 'trending_down' }}
        </mat-icon>
        <span>{{ trendPercent }}%</span>
      </div>
    </div>
  `,
  styles: [`
    .stat-card {
      background: white;
      border-radius: 12px;
      padding: 24px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
      border: 1px solid #e8ecef;
      display: flex;
      align-items: center;
      gap: 16px;
      transition: all 250ms cubic-bezier(0.4, 0, 0.2, 1);
      position: relative;
      overflow: hidden;

      &::before {
        content: '';
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        height: 4px;
        background: linear-gradient(90deg, #1B7A5E, #13C784);
      }

      &:hover {
        transform: translateY(-4px);
        box-shadow: 0 8px 16px rgba(0, 0, 0, 0.12);
      }

      &.color-primary {
        &::before {
          background: linear-gradient(90deg, #1B7A5E, #13664e);
        }
      }

      &.color-accent {
        &::before {
          background: linear-gradient(90deg, #13C784, #0fb375);
        }
      }

      &.color-warn {
        &::before {
          background: linear-gradient(90deg, #f44336, #e53935);
        }
      }

      &.color-info {
        &::before {
          background: linear-gradient(90deg, #2196f3, #1976d2);
        }
      }
    }

    .stat-icon {
      font-size: 32px;
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: rgba(27, 122, 94, 0.1);
      border-radius: 8px;

      mat-icon {
        font-size: 24px;
        width: 24px;
        height: 24px;
      }
    }

    .stat-info {
      flex: 1;
    }

    .stat-value {
      font-size: 24px;
      font-weight: 700;
      color: #1a1a1a;
      line-height: 1.2;
    }

    .stat-title {
      font-size: 13px;
      color: #999;
      font-weight: 500;
      margin-top: 4px;
    }

    .stat-badge {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 12px;
      font-weight: 600;
      padding: 6px 12px;
      border-radius: 20px;

      mat-icon {
        font-size: 16px;
        width: 16px;
        height: 16px;
      }

      &.trend-up {
        background-color: #e8f5e9;
        color: #13C784;
      }

      &.trend-down {
        background-color: #ffebee;
        color: #f44336;
      }
    }
  `]
})
export class StatCardComponent {
  @Input() title: string = 'Stat';
  @Input() value: string = '0';
  @Input() icon: string = 'info';
  @Input() color: 'primary' | 'accent' | 'warn' | 'info' = 'primary';
  @Input() trend: 'up' | 'down' | null = null;
  @Input() trendPercent: number = 0;
}
