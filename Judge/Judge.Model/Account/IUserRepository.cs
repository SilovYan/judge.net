﻿using System.Collections.Generic;
using Judge.Model.Entities;

namespace Judge.Model.Account
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers(IEnumerable<long> users);

        User GetUser(long id);
    }
}