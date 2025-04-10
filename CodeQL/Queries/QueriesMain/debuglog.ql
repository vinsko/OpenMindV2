import csharp

/**
 * Query to find any occurrences of Debug.Log method calls in C#
 */
from MethodAccess call
where
  call.getMethod().getDeclaringType().getName() = "Debug" and
  call.getMethod().getName() = "Log"
select call, "Detected a call to Debug.Log"