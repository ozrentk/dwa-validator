using System;
using System.IO;

namespace Utils
{
    public class FluentPath
    {
        private string _path;

        private FluentPath(string path)
        {
            _path = path;
        }

        public static FluentPath From(string path) => new FluentPath(path);

        public FluentPath PrependCombined(params string[] parts)
        {
            _path = Path.Combine(parts.Append(_path).ToArray());
            return this;
        }

        public FluentPath AppendCombined(params string[] parts)
        {
            _path = Path.Combine(new[] { _path }.Concat(parts).ToArray());
            return this;
        }

        public FluentPath NormalizeToUnix()
        {
            _path = _path.Replace('\\', '/');
            return this;
        }

        public FluentPath NormalizeToWindows()
        {
            _path = _path.Replace('/', '\\');
            return this;
        }

        public FluentPath Replace(string oldValue, string? newValue)
        {
            _path = _path.Replace(oldValue, newValue);
            return this;
        }

        public FluentPath GetFullPath()
        {
            _path = Path.GetFullPath(_path);
            return this;
        }

        public bool IsRooted => Path.IsPathRooted(_path);

        public string GetExtension() => Path.GetExtension(_path);

        public string GetFileName() => Path.GetFileName(_path);

        public string GetFileNameWithoutExtension() => Path.GetFileNameWithoutExtension(_path);

        public string GetDirectoryName() => Path.GetDirectoryName(_path);

        public bool Exists()
        {
            return File.Exists(_path) || Directory.Exists(_path);
        }

        public string Value => _path;

        public override string ToString() => _path;
    }
}
