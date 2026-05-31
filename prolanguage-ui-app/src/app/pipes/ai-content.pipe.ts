import { Pipe, PipeTransform } from '@angular/core';

/**
 * Renders a minimal subset of markdown coming from AI Tutor replies as safe HTML.
 *
 * Supported: **bold** -> <strong>bold</strong>.
 *
 * The text is HTML-escaped BEFORE any markdown is applied, so nothing the AI
 * emits is ever interpreted as markup — only the <strong> tags this pipe adds
 * are real HTML. This keeps the output safe to bind with [innerHTML].
 */
@Pipe({
  name: 'aiContent',
  standalone: true
})
export class AiContentPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) {
      return '';
    }

    const escaped = value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');

    // **bold** -> <strong>bold</strong>. Non-greedy so multiple spans on one
    // line match independently; an unmatched ** is left as literal text.
    return escaped.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
  }
}
