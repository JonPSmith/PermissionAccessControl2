using System;

namespace DataAuthorize
{
    public interface IOwnedBy
    {
        string OwnedBy { get; }
        void SetOwnedBy(string protectKey);
    }
}