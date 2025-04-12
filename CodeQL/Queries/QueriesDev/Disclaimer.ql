/**
 * @name Missing required header comments
 * @description Ensures every C# file contains two specific comments at the top.
 * @kind problem
 * @problem.severity error
 */

import csharp

/**
 * The required text for the first and second comments.
 */
string requiredFirstCommentText = 
  "This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.";

string requiredSecondCommentText = 
  "© Copyright Utrecht University (Department of Information and Computing Sciences)";

/**
 * Class representing a file missing the required first or second comment.
 */
class FileMissingRequiredComments extends File {
  FileMissingRequiredComments() {
    not exists(Comment firstComment, Comment secondComment |
      firstComment.getFile() = this and
      secondComment.getFile() = this and
      firstComment.getLocation().getStartLine() = 1 and
      secondComment.getLocation().getStartLine() = 2 and
      firstComment.getText().regexpMatch("^" + requiredFirstCommentText) and
      secondComment.getText().regexpMatch("^" + requiredSecondCommentText)
    )
  }
}

from FileMissingRequiredComments file
select file, "This file is missing the required header comments."
