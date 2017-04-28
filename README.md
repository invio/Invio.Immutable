# Invio.Immutable

[![Appveyor](https://ci.appveyor.com/api/projects/status/gop4980i5778689c/branch/master?svg=true)](https://ci.appveyor.com/project/invio/invio-immutable/branch/master)
[![Travis CI](https://img.shields.io/travis/invio/Invio.Immutable.svg?maxAge=3600&label=travis)](https://travis-ci.org/invio/Invio.Immutable)
[![NuGet](https://img.shields.io/nuget/v/Invio.Immutable.svg)](https://www.nuget.org/packages/Invio.Immutable/)
[![Coverage](https://coveralls.io/repos/github/invio/Invio.Immutable/badge.svg?branch=master)](https://coveralls.io/github/invio/Invio.Immutable?branch=master)

This library simplifies the create and management of immutable value objects in C#. This is done by having value objects inherit from a single `ImmutableBase<T>` implementation which inspects the child class to automatically provide appropriate `Equals()`, `GetHashCode()`, and `ToString()` overloads as well as convenient setters and getters that limit boilerplate code.

# Installation
The latest version of this package is available on NuGet. To install, run the following command:

```
PM> Install-Package Invio.Immutable
```

## Basic Usage

Without this library, creating an immutable value object in C# takes a lot of boilerplate code. For example, creating an immutable `User` class could look something like the following:

```cs

public class User : IEquatable<User> {

    public Guid Id { get; }
    public String Name { get; }
    public DateTime Created { get; }
    
    public User(Guid id, String name, DateTime created) {
        this.Id = id;
        this.Name = name;
        this.Created = created;
    }
    
    public override int GetHashCode() {
        return this.Id.GetHashCode() ^  (this.Name?.GetHashCode() ?? 0) ^ this.Created.GetHashCode();
    }
    
    public override bool Equals(object that) {
        return this.Equals(that as User);
    }
    
    public bool Equals(User that) {
        return that != null && this.Id == that.Id && this.Name == that.Name && this.Created == that.Created;
    }
    
    public User SetId(Guid id) {
        return new User(id, this.Name, this.Created);
    }
    
    public User SetName(String name) {
        return new User(this.Id, name, this.Created);
    }
    
    public User SetCreated(DateTime created) {
        return new User(this.Id, this.Name, created);
    }
    
    public override String ToString() {
        return $"{{ Id: {this.Id}, Name: {this.Name}, Created: {this.Created} }}"; 
    }

}
```

This allows every instance of `User` to be compared against any other instance of `User` based upon the values of its properties, but with numerous headaches:

1. Everytime a property is removed or renamed, all of the set, equality, hashing, and string conversion implementations need to be updated to reflect the updates to the property.
2. The `GetHashCode()`, `Equals()`, and `ToString()` implementations are predictable implementations of a pattern.
3. Properties that store reference types need to add branching logic in order to dance around null.

Here is an alternative implementation of `User` using the `ImmutableBase<TImmutable>` class found in this library would result in the following:

```cs

public class User : ImmutableBase<User> {

    public Guid Id { get; }
    public String Name { get; }
    public DateTime Created { get; }
    
    public User(Guid id, String name, DateTime created) {
        this.Id = id;
        this.Name = name;
        this.Created = created;
    }
    
    public User SetId(Guid id) {
        return this.SetPropertyValueImpl(nameof(Id), id);
    }
    
    public User SetName(String name) {
        return this.SetPropertyValueImpl(nameof(Name), name);
    }
    
    public User SetCreated(DateTime created) {
        return this.SetPropertyValueImpl(nameof(DateTime), created);
    }

}
```

This solves all of the headaches referenced above.

1. The name and type of each property are used to identify the which constructor parameter is associated with that property. Updating a property will only result in updating the class members that revolve around that property/
2. The `Equals()`, `GetHashCode()` and `ToString()` implementations automatically inspect the values of each of the property to fulfill their contracts.
3. There is no special logic for properties that store reference types as opposed to value types.
