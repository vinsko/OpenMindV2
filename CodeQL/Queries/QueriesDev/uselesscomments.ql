import csharp

/**
 * Query to detect useless comments in C# code
 * This detects:
 * - Single-word comments (like TODO, fix, etc.)
 * - Empty or very short comments
 * - Comments that repeat code logic
 */

class UselessComment extends Comment {
  UselessComment() {
    exists(
      this.getText().regexpMatch("(?i)\\b(TODO|FIX|TEST|CHECK|DEBUG|HACK)\\b") or
      this.getText().regexpMatch("^\\s*$") or // Empty comment
      this.getText().regexpMatch("^\\s*//\\s*\\w+\\s*$") // Single-word comment
    )
  }
}

from UselessComment c
select c, "Detected a useless comment"