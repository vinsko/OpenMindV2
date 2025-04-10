/**
 * @name Missing comments every 15 lines
 * @description Finds regions in a file with more than 15 lines without a comment.
 * @kind problem
 * @problem.severity warning
 */

import csharp
import semmle.code.csharp.dataflow.TaintTracking

/**
 * Class to represent gaps between comments
 */
class CommentGap extends TopLevel {

  CommentGap() {
    exists (int line1, int line2 |
      this.hasLocationInfo(_, _, line1, _, _, line2) and
      line2 - line1 > 15 and
      not exists(Comment c |
        c.getLocation().getFile() = this.getFile() and
        c.getLocation().getStartLine() > line1 and
        c.getLocation().getStartLine() < line2
      )
    )
  }
}

from CommentGap gap
select gap, "There is a gap of more than 15 lines without any comments."
