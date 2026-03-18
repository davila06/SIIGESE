import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

/**
 * Converts lightweight markdown (bold, italic, lists, line breaks) to safe HTML
 * for display in chat bot messages.
 */
@Pipe({
  name: 'chatMarkdown',
  pure: true,
  standalone: false,
})
export class ChatMarkdownPipe implements PipeTransform {
  constructor(private readonly sanitizer: DomSanitizer) {}

  transform(value: string | null | undefined): SafeHtml {
    if (!value) return '';

    let html = this.escapeHtml(value);

    // Bold: **text**
    html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
    // Italic: *text*
    html = html.replace(/(?<!\*)\*([^*]+)\*(?!\*)/g, '<em>$1</em>');
    // Bullet list: lines starting with •
    html = html.replace(/^• (.+)$/gm, '<li>$1</li>');
    // Wrap consecutive <li> elements
    html = html.replace(/(<li>.*<\/li>\n?)+/g, '<ul class="chat-list">$&</ul>');
    // Line breaks
    html = html.replace(/\n/g, '<br>');

    return this.sanitizer.bypassSecurityTrustHtml(html);
  }

  private escapeHtml(text: string): string {
    return text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }
}
