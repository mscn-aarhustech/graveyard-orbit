# Contributing

Start from an issue with a clear, observable outcome. Keep each pull request small enough for a peer to understand in one sitting.

## Fork and collaborate

The teacher repository is the upstream source. Each team chooses one shared team fork; teammates collaborate on branches in that fork with repository access granted by its owner. A student without write access can send a pull request from a personal fork into the team fork.

Clone the team fork, then add the teacher repository as upstream:

```bash
git remote add upstream https://github.com/mscn-aarhustech/graveyard-orbit.git
git fetch upstream
git switch main
git merge --ff-only upstream/main
git push origin main
git switch -c feature/issue-12-fuel-gauge
```

Replace the example branch name with your own issue. Commit small, coherent changes. Push your branch and open a pull request into the team fork's main branch. Ask a teammate to review it. The teacher can request a separate pull request from the team fork back to the upstream repository.

If fast-forwarding fails, stop and inspect the history with your teammate. Do not force-push main or erase another student's work. Branch synchronization and resolving a real conflict are part of the exercise.

## Definition of done

The acceptance criteria work; build and existing tests pass; a relevant new test or manual check is documented; changed controls/assets/setup are documented; a peer has reviewed the code; and no bin/obj outputs, secrets, or package caches are committed.

Reviews should explain why a change is needed, not only suggest syntax. Check body/resource lifetime, units, input timing, filename case, and how the feature behaves after restart.

## AI-assisted work

You may use AI to explain code, propose tests, or help implement a change. Record material assistance in the pull request. You must be able to explain the submitted code and verify that its behavior, dependencies, and licensing fit the project.

Original contributions use the root MIT license. Keep third-party license notices intact.

## Updating dependencies

Change the pinned version in Directory.Packages.props deliberately, then run `dotnet restore GraveyardOrbit.sln --force-evaluate`. Review and commit the affected packages.lock.json files with the version change. Verify a subsequent locked restore, the tests, and native startup before merging. Do not delete lock files to make CI green.
