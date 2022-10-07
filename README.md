# Template Dylint library

This repository is archived. Please use the following command to create new [Dylint](https://github.com/trailofbits/dylint) libraries:

```
cargo dylint --new <path>
```

---

[Dylint](https://github.com/trailofbits/dylint) is a tool for running Rust lints from dynamic libraries. This repository is a "blank slate" Dylint library.

The recommended way to create a new Dylint library is with `cargo dylint --new PATH`.

However, forking this respository and running `./start_new_lint.sh NEW_LINT_NAME` should also work.

The documentation for `start_from_clippy_lint.sh` is retained below. However, the script is currently non-functional and requires updating.

---

**Experimental**

Choose a [Clippy lint](https://rust-lang.github.io/rust-clippy/master/) and run the following two commands:

```sh
./start_from_clippy_lint.sh CLIPPY_LINT_NAME NEW_LINT_NAME
cargo build
```

If the first command fails: sorry. Perhaps try another Clippy lint.

If the first command succeeds, but the second fails: you are probably halfway to having a functional Dylint library.

If both commands succeed: hooray! You might then try the following:

```sh
DYLINT_LIBRARY_PATH=$PWD/target/debug cargo dylint NEW_LINT_NAME -- --manifest-path=PATH_TO_OTHER_PACKAGES_MANIFEST
```
