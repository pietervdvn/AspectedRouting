using System.Collections.Generic;
using System.IO;
using AspectedRouting.Language;

namespace AspectedRouting.IO.LuaSkeleton
{
    
    /// <summary>
    /// The 'LuaSkeleton' is a class which is used in Lua generation of profiles.
    ///
    /// The lua skeleton basically keeps track of dependencies, and added functions.
    /// Once done, all these can be retrieved as code.
    ///
    /// E.g. if an expression is turned into lua with 'ToExpression', then the dependencies will be automatically added.
    /// 
    /// </summary>
    public partial class LuaSkeleton
    {
        private readonly Context _context;

        private readonly HashSet<string> _dependencies = new HashSet<string>();
        private readonly List<string> _functionImplementations = new List<string>();
        private readonly HashSet<string> _alreadyAddedFunctions = new HashSet<string>();

        public LuaSkeleton(Context context)
        {
            _context = context;
        }

        internal void AddDep(string name)
        {
            _dependencies.Add(name);
        }

        public List<string> GenerateFunctions()
        {
            return _functionImplementations;
        }
        
        public void AddDependenciesFor(IExpression e)
        {
            var (_, functionNames) = e.InList().DirectlyAndInderectlyCalled(_context);
            foreach (var functionName in functionNames)
            {

                if (_context.DefinedFunctions.TryGetValue(functionName, out var aspectemeta))
                {
                    AddFunction(aspectemeta);
                }
                else
                {
                    AddDep(functionName);
                }
            }
        }

        public List<string> GenerateDependencies()
        {
            var imps = new List<string>();

            foreach (var name in _dependencies)
            {
                var path = $"IO/lua/{name}.lua";
                if (File.Exists(path))
                {
                    imps.Add(File.ReadAllText(path));
                }
                else
                {
                    throw new FileNotFoundException(path);
                }
            }

            return imps;
        }
        
    }
}