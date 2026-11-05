# Instructor Notes

The repository is intentionally readable, not production-ready. Sample tests establish seams and feedback loops without covering every branch. Treat the end-to-end shot as the first tracer bullet and ask students to explain it before adding systems.

For a short course, require one feature pull request, one review, and one debugging investigation per team. For a longer course, add fuel or prediction and a cross-platform verification exercise. The licensing discussion is separate from the code assessment.

The baseline is working and has no intentional bug seeds. For a controlled debugging exercise, create a separate branch or commit and identify it as an exercise. Candidate mutations are removing the inner-radius clamp, changing sprite rotation offset, swapping input axes, or forgetting to copy a new asset. Keep the baseline branch green and record the answer privately. Do not distribute unexplained defects as if they were verified behavior.

Create real issues from BACKLOG.md. Enable branch protection/rulesets in each team fork if appropriate; neither a ZIP nor a workflow configures these settings. Make reviews and checks required according to the course's collaboration model.

Only the generating environment's results are recorded in VALIDATION.md. The matrix workflow is configured, not proof that GitHub has already run it. Ask a Windows user and a macOS user to perform the manual checklist before declaring those platforms verified.

Useful assessment evidence: a dependency sketch, coherent commits, issue-to-PR linkage, review reasoning, tests targeting a real risk, correct resource lifetime, and an explanation of assisted code.
