namespace IocDemo

module Prelude =

    // These types' fields are not used in this demo - the important characteristic is that they are types that are distinct from each other.
    type Senior = { name : string; department : string }
    type Intern = { name : string }
    type Pair = Intern * Senior
