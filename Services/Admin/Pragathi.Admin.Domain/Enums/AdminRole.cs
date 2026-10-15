namespace Pragathi.Admin.Domain.Enums;

public enum AdminRole
{
    Viewer = 1,     // can only see leads
    Editor = 2,     // can also edit and delete leads
    Superuser = 3   // can also create, edit and delete users
}