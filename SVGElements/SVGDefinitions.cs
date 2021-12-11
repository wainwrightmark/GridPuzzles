using System.Collections.Generic;

namespace SVGElements;

public sealed partial record SVGDefinitions(string Id, IReadOnlyList<SVGElement> Children) : SVGElement("defs",
    Id,
    null,
    Children
);