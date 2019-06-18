using System.Diagnostics;

namespace Erde
{
    public static class Tools
    {
        public static void Verify (object a_object, bool a_state)
        {
            Debug.Assert(a_state, string.Format("[Warning] Resource leaked {0}", a_object.GetType().ToString()));
        }
    }
}
