import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

interface Testimonial {
  name: string;
  role: string;
  date: string;
  rating: number;
  title: string;
  comment: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class HomeComponent implements AfterViewInit, OnDestroy {
  @ViewChild('homeStatsSection', { static: true })
  homeStatsSection!: ElementRef<HTMLElement>;

  @ViewChild('testimonialsTrack')
  testimonialsTrack?: ElementRef<HTMLElement>;

  teachersCount = 0;
  countriesCount = 0;
  studentsCount = 0;
  starIndexes = [0, 1, 2, 3, 4];

  private hasAnimatedStats = false;
  private statsObserver?: IntersectionObserver;
  private animationFrameIds: number[] = [];

  constructor(private router: Router, private authService: AuthService) {}

  goToAiChat(query?: string) {
    if (this.authService.isAuthenticated()) {
      if (query && query.trim()) {
        this.router.navigate(['/ai-chat'], { queryParams: { q: query } });
      } else {
        this.router.navigate(['/ai-chat']);
      }
    } else {
      this.router.navigate(['/signin']);
    }
  }

  partnerLogos: string[] = [
    'goethe',
    'idc',
    'ielts',
    'british-council',
    'pingo',
    'cambridge'
  ];

  testimonials: Testimonial[] = [
    {
      name: 'Anna Kovac',
      role: 'Student',
      date: 'Oct 14, 2025',
      rating: 5,
      title: 'The best Teacher',
      comment:
        'I was always too shy to speak English in public, but the teachers here are so patient. After just 3 months, I can finally hold a real conversation without translating in my head!'
    },
    {
      name: 'Miguel Torres',
      role: 'Student',
      date: 'Nov 02, 2025',
      rating: 5,
      title: 'Passed my B2 exam!',
      comment:
        'I signed up for the intensive German course to prepare for my B2 certification. The grammar explanations were crystal clear and the mock tests helped me pass on my first try.'
    },
    {
      name: 'Sarah Chen',
      role: 'Student',
      date: 'Dec 10, 2025',
      rating: 5,
      title: 'Fun and engaging',
      comment:
        "Pro Language School isn't like my boring high school classes. The lessons are interactive and we actually practice speaking every single day. I'm loving the experience."
    },
    {
      name: 'Daniel Green',
      role: 'Student',
      date: 'Jan 18, 2026',
      rating: 5,
      title: 'My confidence grew fast',
      comment:
        'The speaking-focused lessons made it much easier to talk with clients at work. I feel much more confident in real conversations now.'
    }
  ];

  ngAfterViewInit(): void {
    if (typeof IntersectionObserver === 'undefined') {
      return;
    }

    this.statsObserver = new IntersectionObserver(
      (entries) => {
        const isVisible = entries.some((entry) => entry.isIntersecting);
        if (!isVisible || this.hasAnimatedStats) {
          return;
        }

        this.hasAnimatedStats = true;
        this.startStatsAnimation();
        this.statsObserver?.disconnect();
      },
      {
        threshold: 0.35
      }
    );

    this.statsObserver.observe(this.homeStatsSection.nativeElement);
  }

  ngOnDestroy(): void {
    this.statsObserver?.disconnect();
    for (const frameId of this.animationFrameIds) {
      cancelAnimationFrame(frameId);
    }
    this.animationFrameIds = [];
  }

  scrollTestimonials(direction: 'previous' | 'next'): void {
    const track = this.testimonialsTrack?.nativeElement;
    if (!track) {
      return;
    }

    const card = track.querySelector<HTMLElement>('.testimonial-card');
    const cardWidth = card?.getBoundingClientRect().width ?? 360;
    const computedStyles = window.getComputedStyle(track);
    const gap = Number.parseFloat(computedStyles.columnGap || computedStyles.gap || '24') || 24;
    const offset = cardWidth + gap;
    const maxScrollLeft = Math.max(0, track.scrollWidth - track.clientWidth);
    const edgeTolerance = 4;

    if (direction === 'next' && track.scrollLeft >= maxScrollLeft - edgeTolerance) {
      track.scrollTo({ left: 0, behavior: 'smooth' });
      return;
    }

    if (direction === 'previous' && track.scrollLeft <= edgeTolerance) {
      track.scrollTo({ left: maxScrollLeft, behavior: 'smooth' });
      return;
    }

    track.scrollBy({
      left: direction === 'next' ? offset : -offset,
      behavior: 'smooth'
    });
  }

  private startStatsAnimation(): void {
    this.animateCounter(30, (value) => {
      this.teachersCount = value;
    });

    this.animateCounter(30, (value) => {
      this.countriesCount = value;
    });

    this.animateCounter(2000, (value) => {
      this.studentsCount = value;
    }, 1900);
  }

  private animateCounter(
    target: number,
    update: (value: number) => void,
    duration = 1600
  ): void {
    const startTime = performance.now();

    const step = (timestamp: number) => {
      const elapsed = timestamp - startTime;
      const progress = Math.min(elapsed / duration, 1);
      const easedProgress = 1 - Math.pow(1 - progress, 3);
      const currentValue = Math.round(target * easedProgress);
      update(currentValue);

      if (progress < 1) {
        const frameId = requestAnimationFrame(step);
        this.animationFrameIds.push(frameId);
      }
    };

    const initialFrameId = requestAnimationFrame(step);
    this.animationFrameIds.push(initialFrameId);
  }
}
