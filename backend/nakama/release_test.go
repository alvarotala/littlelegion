package main

import (
	"context"
	"encoding/json"
	"errors"
	"strings"
	"testing"

	"github.com/heroiclabs/nakama-common/runtime"
)

func TestRequiredOnlineRelease(t *testing.T) {
	p := onlineRelease
	if err := requireClientRelease(p.Version, p.Protocol, p.Rules, p.ContentHash); err != nil {
		t.Fatal(err)
	}
	for _, version := range []string{"", "0.1.12", "0.1.13-modified", "99.0.0"} {
		req := searchRequest{GameVersion: version, Faction: "industrial", Protocol: p.Protocol, Rules: p.Rules, ContentHash: p.ContentHash}
		body, _ := json.Marshal(req)
		// Rejection must happen before creating/looking up a queue entry. A nil
		// transaction makes an accidental database access fail this regression.
		_, err := searchOpen(context.Background(), nil, nil, "test-player", string(body))
		var apiError *runtime.Error
		if !errors.As(err, &apiError) || apiError.Code != 9 || !strings.HasPrefix(apiError.Message, "UPDATE_REQUIRED:") {
			t.Fatalf("version %q was not rejected with an update requirement: %v", version, err)
		}
	}
	for _, req := range []releasePolicy{
		{p.Version, p.Protocol - 1, p.Rules, p.ContentHash},
		{p.Version, p.Protocol, p.Rules - 1, p.ContentHash},
		{p.Version, p.Protocol, p.Rules, strings.Repeat("0", 64)},
	} {
		if requireClientRelease(req.Version, req.Protocol, req.Rules, req.ContentHash) == nil {
			t.Fatal("An incompatible client claiming the current version was accepted")
		}
		if requireReleaseContent(req.Protocol, req.Rules, req.ContentHash) == nil {
			t.Fatal("An outdated dedicated server was accepted")
		}
	}
}
