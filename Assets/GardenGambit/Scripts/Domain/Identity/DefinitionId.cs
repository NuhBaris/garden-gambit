using System;

namespace GardenGambit.Domain.Identity
{
    public readonly struct DefinitionId : IEquatable<DefinitionId>
    {
        private readonly string _value;

        public DefinitionId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "DefinitionId cannot be null, empty, or whitespace.",
                    nameof(value));
            }

            _value = value;
        }

        public string Value => _value;

        public bool IsValid => !string.IsNullOrWhiteSpace(_value);

        public bool Equals(DefinitionId other)
        {
            return string.Equals(
                _value,
                other._value,
                StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is DefinitionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value == null
                ? 0
                : StringComparer.Ordinal.GetHashCode(_value);
        }

        public override string ToString()
        {
            return _value ?? string.Empty;
        }

        public static bool operator ==(
            DefinitionId left,
            DefinitionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            DefinitionId left,
            DefinitionId right)
        {
            return !left.Equals(right);
        }
    }
}