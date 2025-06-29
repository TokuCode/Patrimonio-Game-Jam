﻿using UnityEngine;

namespace Movement3D.Helpers
{
    public interface IVisitor
    {
        void Visit<T>(T visitable) where T : Component, IVisitable;
    }

    public interface IVisitable
    {
        void Accept(IVisitor visitor);
    }
}