package main

import (
	"context"
	"database/sql"
	_ "embed"
	"encoding/json"
	"regexp"

	"github.com/heroiclabs/nakama-common/runtime"
)

//go:embed release.json
var releaseJSON []byte

type releasePolicy struct {
	Version     string `json:"version"`
	Protocol    int    `json:"protocol"`
	Rules       int    `json:"rules"`
	ContentHash string `json:"contentHash"`
}

// Fail closed if the image was built without a valid release policy.
var onlineRelease = func() releasePolicy {
	var value releasePolicy
	if json.Unmarshal(releaseJSON, &value) != nil || !regexp.MustCompile(`^[0-9]+\.[0-9]+\.[0-9]+$`).MatchString(value.Version) ||
		value.Protocol < 1 || value.Rules < 1 || !regexp.MustCompile(`^[A-F0-9]{64}$`).MatchString(value.ContentHash) {
		panic("Invalid required online release")
	}
	return value
}()

func requiredReleaseError() error {
	return runtime.NewError("UPDATE_REQUIRED: Install Little Legion "+onlineRelease.Version+" to play online.", 9)
}

func requireReleaseContent(protocol, rules int, hash string) error {
	if protocol != onlineRelease.Protocol || rules != onlineRelease.Rules || hash != onlineRelease.ContentHash {
		return requiredReleaseError()
	}
	return nil
}

func requireClientRelease(version string, protocol, rules int, hash string) error {
	if version != onlineRelease.Version {
		return requiredReleaseError()
	}
	return requireReleaseContent(protocol, rules, hash)
}

func clientPolicy(ctx context.Context, logger runtime.Logger, tx *sql.Tx, user, payload string) (any, error) {
	return onlineRelease, nil
}
