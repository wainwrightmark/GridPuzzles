﻿namespace GridPuzzles.Clues.Constraints;

public abstract class CommutativeConstraint<T> : Constraint<T>
{
    /// <inheritdoc />
    public override Constraint<T> FlippedConstraint => this;
}