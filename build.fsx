#I "packages/build/FAKE/tools"
#r "FakeLib.dll"

open Fake

let configuration = environVarOrDefault "Configuration" (if buildServer = LocalBuild then "Debug" else "Release")

Target "build" <| fun _ ->
    DotNetCli.Build (fun p -> { p with WorkingDir = "src"; Configuration = configuration } )

Target "pack" <| fun _ ->
    DotNetCli.Pack (fun p -> { p with WorkingDir = "src"; Configuration = configuration } )

    !! ("src\\**\\*.nupkg") |> Seq.iter TeamCityHelper.PublishArtifact

Target "ci" DoNothing

"build" ==> "pack"
"pack" ==> "ci"

RunTargetOrDefault "build"
