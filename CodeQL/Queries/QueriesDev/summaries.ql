import csharp

/**
 * Detect if a method declaration has an XML <summary> comment.
 */
class MethodWithoutSummary extends Method {
  MethodWithoutSummary() {
    // The method is missing the XML <summary> tag in its documentation.
    not exists(this.getDocComment().getText().regexpMatch("<summary>"))
  }
}

from MethodWithoutSummary method
select method, "This method is missing a <summary> documentation tag."
