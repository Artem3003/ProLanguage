import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

type BillingPeriod = 'monthly' | 'yearly';

interface PlanFeature {
  text: string;
}

interface Plan {
  name: string;
  monthlyPrice: number;
  yearlyPrice: number;
  highlighted?: boolean;
  features: PlanFeature[];
}

@Component({
  selector: 'app-pricing',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pricing.html',
  styleUrl: './pricing.scss'
})
export class PricingComponent {
  billing: BillingPeriod = 'monthly';

  readonly plans: Plan[] = [
    {
      name: 'Basic',
      monthlyPrice: 0,
      yearlyPrice: 0,
      features: [
        { text: 'Free Courses' },
        { text: 'Community Support' }
      ]
    },
    {
      name: 'Standart',
      monthlyPrice: 999,
      yearlyPrice: 9990,
      highlighted: true,
      features: [
        { text: 'All Basic Features' },
        { text: 'Paid Courses' },
        { text: 'AI Tutor' },
        { text: '2 Lessons with Teacher' },
        { text: 'Certificate of Completion' }
      ]
    },
    {
      name: 'Premium',
      monthlyPrice: 3999,
      yearlyPrice: 39990,
      features: [
        { text: 'All Standart Features' },
        { text: '8 Lessons with Teacher' },
        { text: 'Personalized learning plan' }
      ]
    }
  ];

  setBilling(mode: BillingPeriod): void {
    this.billing = mode;
  }

  getPrice(plan: Plan): number {
    return this.billing === 'monthly' ? plan.monthlyPrice : plan.yearlyPrice;
  }

  getPeriodLabel(): string {
    return this.billing === 'monthly' ? '/mo' : '/yr';
  }
}
