﻿// -----------------------------------------------------------------------
//  <copyright file="IProvider.cs" company="Asynkron HB">
//      Copyright (C) 2015-2017 Asynkron HB All rights reserved
//  </copyright>
// -----------------------------------------------------------------------

namespace Proto.Persistence
{
    public interface IProvider
    {
        IProviderState GetState();
    }
}