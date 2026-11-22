using System;

namespace MyWebAPI.Models;

[Flags]
public enum Roles
{
    None = 0,
    User = 1 << 0,
    Manager = 1 << 1,
    Admin = 1 << 2,
}