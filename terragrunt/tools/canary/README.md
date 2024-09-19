# CDP SIRSI Canary

We are using scripts in this folder to perform different synthetic tests against the application.

## Install

```shell
make install
```

## Package

```shell
make package
```

## Deploy

> In the following examples ave is alias for `aws-vault exec` command.
Feel free to use any convenient AWS profiler instead.
```shell
aws-switch-to-cdp-sirsi-orchestrator-goaco-terraform
ave make deploy
```