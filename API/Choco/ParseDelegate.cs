namespace Chocolatey;

/// <summary>
/// A method that parses a <see langword="string"/> into the specified <typeparamref name="T"/>.
/// </summary>
public delegate T Parse<T>(string str);
