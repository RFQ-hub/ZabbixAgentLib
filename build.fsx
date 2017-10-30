#I "packages/build/FAKE/tools"
#r "FakeLib.dll"

open Fake

let configuration = environVarOrDefault "Configuration" (if buildServer = LocalBuild then "Debug" else "Release")
let solution = "ZabbixAgentLib.sln"

Target "build" <| fun _ ->
    DotNetCli.Build (fun p -> { p with WorkingDir = "src"; Configuration = configuration; Project = solution })

Target "pack" <| fun _ ->
    DotNetCli.Pack (fun p -> { p with WorkingDir = "src"; Configuration = configuration; Project = solution })

    !! ("src\\**\\*.nupkg") |> Seq.iter TeamCityHelper.PublishArtifact

Target "ci" DoNothing

"build" ==> "pack"
"pack" ==> "ci"

RunTargetOrDefault "build"
