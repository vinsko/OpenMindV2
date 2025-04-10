import csharp

/**
 * Query to enforce C# naming conventions
 *  - UpperCamelCase: Types, namespaces, interfaces, methods, properties, events, non-private fields, constants, static read-only fields, enum members, local functions.
 *  - Interfaces should start with "I".
 *  - Type parameters should start with "T".
 *  - LowerCamelCase: Local variables, constants, parameters.
 *  - _LowerCamelCase: Private instance/static fields.
 */

// Function to check if a name follows UpperCamelCase
predicate isUpperCamelCase(string name) {
  name =~ "^[A-Z][a-zA-Z0-9]*$"
}

// Function to check if a name follows lowerCamelCase
predicate isLowerCamelCase(string name) {
  name =~ "^[a-z][a-zA-Z0-9]*$"
}

// Function to check if a name follows _lowerCamelCase (for private fields)
predicate isPrivateFieldNaming(string name) {
  name =~ "^_[a-z][a-zA-Z0-9]*$"
}

// Function to check if interface names start with 'I'
predicate isInterfaceNaming(string name) {
  name =~ "^I[A-Z][a-zA-Z0-9]*$"
}

// Function to check if type parameters start with 'T'
predicate isTypeParameterNaming(string name) {
  name =~ "^T[A-Z][a-zA-Z0-9]*$"
}

// Class/Interface name check (UpperCamelCase, interface should start with 'I')
from Class c
where not isUpperCamelCase(c.getName())
select c, "Class name does not follow UpperCamelCase."

from Interface i
where not isInterfaceNaming(i.getName())
select i, "Interface name should start with 'I' and follow UpperCamelCase."

// Type parameters (Should start with 'T')
from TypeParameter t
where not isTypeParameterNaming(t.getName())
select t, "Type parameters should start with 'T' and follow UpperCamelCase."

// Methods, Properties, Events, Non-private fields, Enum members (UpperCamelCase)
from Entity e
where (e instanceof Method or e instanceof Property or e instanceof Event or e instanceof EnumConstant or 
       (e instanceof Field and not e.hasModifier("private"))) and
      not isUpperCamelCase(e.getName())
select e, "Method, property, event, non-private field, or enum member does not follow UpperCamelCase."

// Local variables, constants, and parameters (LowerCamelCase)
from LocalVariable lv
where not isLowerCamelCase(lv.getName())
select lv, "Local variable does not follow lowerCamelCase."

from Parameter p
where not isLowerCamelCase(p.getName())
select p, "Parameter does not follow lowerCamelCase."

from ConstantField cf
where not isUpperCamelCase(cf.getName()) // Constants follow UpperCamelCase
select cf, "Constant field does not follow UpperCamelCase."

// Private instance/static fields (Should follow _lowerCamelCase)
from Field f
where f.hasModifier("private") and not isPrivateFieldNaming(f.getName())
select f, "Private field does not follow _lowerCamelCase."